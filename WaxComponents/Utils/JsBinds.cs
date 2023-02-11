using Microsoft.JSInterop;

namespace WaxComponents.Utils;

public static class JsBinds
{
    public static async void SetImage(this IJSRuntime runtime, Stream stream, string elementId)
    {
        var dotnetStream = new DotNetStreamReference(stream);
        await runtime.InvokeVoidAsync("setImage", elementId, dotnetStream);
    }
}