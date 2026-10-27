// Browser helpers for WaxPopover: viewport-aware floating positioning (flip + shift),
// trigger interaction listeners, and outside click / Escape / scroll bridging back to
// Blazor. Imported by WaxPopoverJs (./_content/WaxComponents/js/popover.js).
//
// `sync` is the single entry point the component calls after every render. It
// reconciles listeners against the requested state and, while open, positions the
// panel and returns the placement that ended up applied (which may have flipped);
// Blazor uses that to render the matching arrow. Scroll/resize repositioning happens
// here without a .NET roundtrip; a scroll-driven flip notifies Blazor once.

interface WaxPopoverCallback {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<void>;
}

export interface WaxPopoverSyncOptions {
    open: boolean;
    trigger: "click" | "hover" | "focus" | "manual";
    placement: string; // "bottom-start" | "top" | ... (side + optional start/end)
    offset: number;
    arrow: boolean;
}

interface PopoverState {
    triggerEl: HTMLElement;
    panel: HTMLElement;
    arrow: HTMLElement | null;
    dotnet: WaxPopoverCallback;
    opts: WaxPopoverSyncOptions;
    boundTrigger: string | null;
    applied: string | null;
    removers: (() => void)[];
    triggerRemovers: (() => void)[];
    closeTimer: number | null;
}

type Side = "top" | "bottom" | "left" | "right";

const states = new WeakMap<HTMLElement, PopoverState>();

const VIEWPORT_MARGIN = 8;
const HOVER_CLOSE_DELAY = 120;
const ARROW = 8; // px square (size-2); rotated 45° it protrudes ARROW/2 past the edge

function on<T extends Event>(
    target: EventTarget,
    type: string,
    handler: (e: T) => void,
    removers: (() => void)[],
    capture = false,
): void {
    const listener: EventListener = (e) => handler(e as T);
    target.addEventListener(type, listener, capture);
    removers.push(() => target.removeEventListener(type, listener, capture));
}

function clamp(value: number, lo: number, hi: number): number {
    return Math.min(Math.max(value, lo), Math.max(lo, hi));
}

function cancelCloseTimer(state: PopoverState): void {
    if (state.closeTimer !== null) {
        clearTimeout(state.closeTimer);
        state.closeTimer = null;
    }
}

function scheduleClose(state: PopoverState): void {
    if (state.closeTimer !== null) return;
    state.closeTimer = window.setTimeout(() => {
        state.closeTimer = null;
        void state.dotnet.invokeMethodAsync("OnWaxPopoverTriggerLeave");
    }, HOVER_CLOSE_DELAY);
}

function leavesPopover(state: PopoverState, related: EventTarget | null): boolean {
    if (!(related instanceof Node)) return true;
    return !state.panel.contains(related) && !state.triggerEl.contains(related);
}

function bindTrigger(state: PopoverState): void {
    for (const remove of state.triggerRemovers) remove();
    state.triggerRemovers = [];
    const { triggerEl, panel, dotnet } = state;

    switch (state.opts.trigger) {
        case "click":
            on(triggerEl, "click", () => void dotnet.invokeMethodAsync("OnWaxPopoverTriggerActivate"), state.triggerRemovers);
            break;

        case "hover":
            on(triggerEl, "mouseenter", () => {
                cancelCloseTimer(state);
                void dotnet.invokeMethodAsync("OnWaxPopoverTriggerEnter");
            }, state.triggerRemovers);
            on(triggerEl, "mouseleave", () => scheduleClose(state), state.triggerRemovers);
            on(triggerEl, "focusin", () => {
                cancelCloseTimer(state);
                void dotnet.invokeMethodAsync("OnWaxPopoverTriggerFocus");
            }, state.triggerRemovers);
            on(triggerEl, "focusout", (e: FocusEvent) => {
                if (leavesPopover(state, e.relatedTarget)) scheduleClose(state);
            }, state.triggerRemovers);
            on(panel, "mouseenter", () => cancelCloseTimer(state), state.triggerRemovers);
            on(panel, "mouseleave", () => scheduleClose(state), state.triggerRemovers);
            break;

        case "focus":
            on(triggerEl, "focusin", () => void dotnet.invokeMethodAsync("OnWaxPopoverTriggerFocus"), state.triggerRemovers);
            on(triggerEl, "focusout", (e: FocusEvent) => {
                if (leavesPopover(state, e.relatedTarget)) {
                    void dotnet.invokeMethodAsync("OnWaxPopoverTriggerBlur");
                }
            }, state.triggerRemovers);
            break;
    }
}

function bindFloating(state: PopoverState): void {
    if (state.removers.length > 0) return;
    const { panel, triggerEl, dotnet } = state;

    on(document, "pointerdown", (e) => {
        const target = e.target;
        if (target instanceof Node && !panel.contains(target) && !triggerEl.contains(target)) {
            void dotnet.invokeMethodAsync("OnWaxPopoverOutsidePointer");
        }
    }, state.removers);

    on(document, "keydown", (e: KeyboardEvent) => {
        if (e.key === "Escape") void dotnet.invokeMethodAsync("OnWaxPopoverEscape");
    }, state.removers);

    // Capture phase: scroll events don't bubble, so this is how nested scroll
    // containers keep the anchored panel glued to its trigger.
    on(window, "scroll", () => {
        const applied = applyPosition(state);
        if (applied !== state.applied) {
            state.applied = applied;
            void state.dotnet.invokeMethodAsync("OnWaxPopoverAppliedPlacement", applied);
        }
    }, state.removers, true);

    on(window, "resize", () => applyPosition(state), state.removers);
}

function unbindFloating(state: PopoverState): void {
    cancelCloseTimer(state);
    for (const remove of state.removers) remove();
    state.removers = [];
}

function fitsOnSide(side: Side, rect: DOMRect, pw: number, ph: number, offset: number): boolean {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const m = VIEWPORT_MARGIN;
    switch (side) {
        case "bottom": return rect.bottom + offset + ph <= vh - m;
        case "top": return rect.top - offset - ph >= m;
        case "right": return rect.right + offset + pw <= vw - m;
        case "left": return rect.left - offset - pw >= m;
    }
}

function applyPosition(state: PopoverState): string {
    const { triggerEl, panel, arrow, opts } = state;
    const rect = triggerEl.getBoundingClientRect();
    const pw = panel.offsetWidth;
    const ph = panel.offsetHeight;
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const m = VIEWPORT_MARGIN;
    const offset = opts.offset;

    const [sideToken, alignToken] = opts.placement.split("-");
    let side: Side = sideToken as Side;
    const align = alignToken ?? "center";

    const opposite: Record<Side, Side> = { top: "bottom", bottom: "top", left: "right", right: "left" };
    if (!fitsOnSide(side, rect, pw, ph, offset) && fitsOnSide(opposite[side], rect, pw, ph, offset)) {
        side = opposite[side];
    }

    const mainCoord = (): number => {
        switch (side) {
            case "bottom": return rect.bottom + offset;
            case "top": return rect.top - offset - ph;
            case "right": return rect.right + offset;
            case "left": return rect.left - offset - pw;
        }
    };

    const crossCoord = (): number => {
        if (side === "top" || side === "bottom") {
            if (align === "start") return rect.left;
            if (align === "end") return rect.right - pw;
            return rect.left + rect.width / 2 - pw / 2;
        }
        if (align === "start") return rect.top;
        if (align === "end") return rect.bottom - ph;
        return rect.top + rect.height / 2 - ph / 2;
    };

    let x: number;
    let y: number;
    if (side === "top" || side === "bottom") {
        y = mainCoord();
        x = crossCoord();
        y = clamp(y, m, vh - m - ph);
        x = clamp(x, m, vw - m - pw);
    } else {
        x = mainCoord();
        y = crossCoord();
        x = clamp(x, m, vw - m - pw);
        y = clamp(y, m, vh - m - ph);
    }

    panel.style.left = `${Math.round(x)}px`;
    panel.style.top = `${Math.round(y)}px`;

    if (arrow !== null) {
        if (side === "top" || side === "bottom") {
            const a = clamp(rect.left + rect.width / 2 - x - ARROW / 2, 8, pw - 8 - ARROW);
            arrow.style.left = `${Math.round(a)}px`;
            arrow.style.right = "";
            if (side === "bottom") {
                arrow.style.top = `${-ARROW / 2}px`;
                arrow.style.bottom = "";
            } else {
                arrow.style.bottom = `${-ARROW / 2}px`;
                arrow.style.top = "";
            }
        } else {
            const a = clamp(rect.top + rect.height / 2 - y - ARROW / 2, 8, ph - 8 - ARROW);
            arrow.style.top = `${Math.round(a)}px`;
            arrow.style.bottom = "";
            if (side === "right") {
                arrow.style.left = `${-ARROW / 2}px`;
                arrow.style.right = "";
            } else {
                arrow.style.right = `${-ARROW / 2}px`;
                arrow.style.left = "";
            }
        }
    }

    return align === "center" ? side : `${side}-${align}`;
}

// Reconciles the popover's browser-side state with what Blazor rendered. Idempotent:
/// safe to call after every render. Returns the applied placement while open.
export function sync(
    trigger: HTMLElement,
    panel: HTMLElement,
    arrow: HTMLElement | null,
    dotnet: WaxPopoverCallback,
    options: WaxPopoverSyncOptions,
): string | null {
    let state = states.get(panel);
    if (!state) {
        state = {
            triggerEl: trigger,
            panel,
            arrow,
            dotnet,
            opts: options,
            boundTrigger: null,
            applied: null,
            removers: [],
            triggerRemovers: [],
            closeTimer: null,
        };
        states.set(panel, state);
    }

    state.triggerEl = trigger;
    state.arrow = arrow;
    state.opts = options;

    if (state.boundTrigger !== options.trigger) {
        bindTrigger(state);
        state.boundTrigger = options.trigger;
    }

    if (options.open) {
        bindFloating(state);
        const applied = applyPosition(state);
        state.applied = applied;
        return applied;
    }

    unbindFloating(state);
    state.applied = null;
    return null;
}

export function detach(panel: HTMLElement): void {
    const state = states.get(panel);
    if (!state) return;
    unbindFloating(state);
    for (const remove of state.triggerRemovers) remove();
    states.delete(panel);
}
