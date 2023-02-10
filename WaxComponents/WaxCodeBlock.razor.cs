using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WaxComponents;

public partial class WaxCodeBlock
{
    [Parameter] public string Code { get; set; } = String.Empty;
    [Parameter] public string FileName { get; set; } = "file.txt";
    [Parameter] public string Language { get; set; } = "txt";
    [Parameter] public string Style { get; set; } = String.Empty;
    [Parameter] public bool Minimal { get; set; }

    private string Lines
    {
        get
        {
            var lines = new List<string>();

            foreach (int line in Enumerable.Range(1, Code.Split("\n").Length))
                lines.Add(line.ToString()
                    .PadLeft(Code.Split("\n").Length.ToString().Length - line.ToString().Length, ' '));

            return string.Join("\n", lines);
        }
    }
}