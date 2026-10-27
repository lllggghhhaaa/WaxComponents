/* WaxColorPicker browser support module.
 *
 * Loaded lazily by WaxColorPickerJs as ./_content/WaxComponents/js/colorPicker.js:
 * - beginPointerDrag: captures the pointer so the saturation/value field keeps reporting moves
 *   outside its bounds, and returns the field's rect so the C# side can normalize client coords.
 * - pickColorWithEyedropper: wraps the EyeDropper API (Chromium-only); reports unsupported
 *   explicitly instead of throwing.
 * - copyText / blurElement: small DOM helpers the component cannot do from C# alone.
 */

export interface DragRect {
    left: number;
    top: number;
    width: number;
    height: number;
}

export interface EyedropperResult {
    color: string | null;
    supported: boolean;
}

type EyeDropperConstructor = new () => { open: () => Promise<{ sRGBHex: string }> };

export function beginPointerDrag(element: HTMLElement, pointerId: number): DragRect {
    element.setPointerCapture(pointerId);
    const rect = element.getBoundingClientRect();
    return { left: rect.left, top: rect.top, width: rect.width, height: rect.height };
}

export function isEyedropperSupported(): boolean {
    return "EyeDropper" in window;
}

export async function pickColorWithEyedropper(): Promise<EyedropperResult> {
    const constructor = (window as Window & { EyeDropper?: EyeDropperConstructor }).EyeDropper;
    if (!constructor) {
        return { color: null, supported: false };
    }

    try {
        const result = await new constructor().open();
        return { color: result.sRGBHex, supported: true };
    } catch {
        // The user dismissed the eyedropper.
        return { color: null, supported: true };
    }
}

export async function copyText(text: string): Promise<boolean> {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        return false;
    }
}

export function blurElement(element: HTMLElement): void {
    element.blur();
}

/* Blazor handlers run after the browser has dispatched the event, so they cannot call
 * preventDefault; this native listener stops arrow keys from scrolling the page while the
 * user is stepping through the saturation field or the format list. Idempotent per element. */
export function preventArrowKeyScroll(element: HTMLElement): void {
    if (element.dataset.waxArrowGuard === "true") {
        return;
    }

    element.dataset.waxArrowGuard = "true";
    element.addEventListener("keydown", (event) => {
        if (event.key.startsWith("Arrow")) {
            event.preventDefault();
        }
    });
}
