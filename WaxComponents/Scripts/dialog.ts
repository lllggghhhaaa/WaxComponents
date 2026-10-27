interface WaxDialogCallback {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<void>;
}

export interface WaxDialogOpenOptions {
    modal: boolean;
    closeOnBackdrop: boolean;
}

interface DialogState {
    removers: (() => void)[];
    modal: boolean;
    dotnet: WaxDialogCallback;
}

const states = new WeakMap<HTMLDialogElement, DialogState>();

let modalCount = 0;
let lockedOverflow: string | null = null;

function lockScroll(): void {
    if (modalCount++ > 0) return;
    const root = document.documentElement;
    lockedOverflow = root.style.overflow;
    root.style.overflow = "hidden";
}

function unlockScroll(): void {
    if (modalCount === 0 || --modalCount > 0) return;
    document.documentElement.style.overflow = lockedOverflow ?? "";
    lockedOverflow = null;
}

function on<T extends Event>(target: EventTarget, type: string, handler: (e: T) => void, removers: (() => void)[]): void {
    const listener: EventListener = (e) => handler(e as T);
    target.addEventListener(type, listener);
    removers.push(() => target.removeEventListener(type, listener));
}

function teardown(dialog: HTMLDialogElement): void {
    const state = states.get(dialog);
    if (!state) return;
    states.delete(dialog);
    for (const remove of state.removers) remove();
}

function closeOut(dialog: HTMLDialogElement, notify: boolean): void {
    const state = states.get(dialog);
    if (!state) return;
    teardown(dialog);
    if (state.modal) unlockScroll();
    if (notify) void state.dotnet.invokeMethodAsync("OnWaxDialogClosed");
}

export function show(
    dialog: HTMLDialogElement,
    dotnet: WaxDialogCallback,
    options: WaxDialogOpenOptions,
): void {
    if (dialog.open || states.has(dialog)) return;

    const removers: (() => void)[] = [];
    states.set(dialog, { removers, modal: options.modal, dotnet });

    on(dialog, "cancel", (e) => {
        e.preventDefault();
        void dotnet.invokeMethodAsync("OnWaxDialogCancel");
    }, removers);

    on(dialog, "close", () => closeOut(dialog, true), removers);

    if (options.modal) {
        on(dialog, "click", (e) => {
            if (options.closeOnBackdrop && e.target === dialog) {
                void dotnet.invokeMethodAsync("OnWaxDialogBackdropClick");
            }
        }, removers);
        lockScroll();
        dialog.showModal();
    } else {
        on(dialog, "keydown", (e: KeyboardEvent) => {
            if (e.key === "Escape") {
                void dotnet.invokeMethodAsync("OnWaxDialogCancel");
            }
        }, removers);
        dialog.show();
    }
}

export function hide(dialog: HTMLDialogElement): void {
    if (dialog.open) {
        dialog.close();
    } else {
        closeOut(dialog, true);
    }
}

export function dispose(dialog: HTMLDialogElement): void {
    if (!states.has(dialog)) return;
    closeOut(dialog, false);
    if (dialog.open) dialog.close();
}
