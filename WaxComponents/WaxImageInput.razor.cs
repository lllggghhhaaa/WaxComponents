using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WaxComponents;

public partial class WaxImageInput : IDisposable
{
    [Parameter] public string Width { get; set; } = "128px";

    [Parameter] public string Height { get; set; } = "128px";
    [Parameter] public string Alt { get; set; } = String.Empty;

    [Parameter] public string DefaultImageUrl { get; set; } = "_content/WaxComponents/Images/upload.svg";

    [Parameter] public EventHandler<WaxFileUploadEventArgs>? OnUpload { get; set; }

    public string? Url { get; private set; }

    private ElementReference _inputRef;
    private ElementReference _imgRef;

    public async void Click() => await JSRuntime.InvokeVoidAsync("inputClick", _inputRef);

    private async void FileUploaded(ChangeEventArgs args)
    {
        Url = await JSRuntime.InvokeAsync<string>("setImage", _inputRef, _imgRef);
        OnUpload?.Invoke(this, new WaxFileUploadEventArgs(Url));
    }

    public void Dispose()
    {
        if (Url is not null)
            JSRuntime.InvokeVoidAsync("disposeObjectURL", Url);
    }
}

public class WaxFileUploadEventArgs : EventArgs
{
    public readonly string Url;

    public WaxFileUploadEventArgs(string url) => Url = url;
        
    public async Task<Stream> DownloadFileAsync()
    {
        using HttpClient client = new HttpClient();
        return await client.GetStreamAsync(Url);
    }
}