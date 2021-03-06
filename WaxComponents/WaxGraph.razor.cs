using System.Drawing;
using System.Globalization;
using System.Numerics;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace WaxComponents;

public partial class WaxGraph
{
    [Parameter] public int Height { get; set; } = 250;

    [Parameter] public int Space { get; set; } = 100;

    [Parameter] public string Style { get; set; } = String.Empty;

    [Parameter, EditorRequired] public List<IGraphItem> Items { get; set; } = null!;

    private BECanvasComponent? _canvas;
    private Canvas2DContext? _context;
    private PointInfo? _viewPoint;
    private int _width = 600;
    private int GraphHeight => Height - 100;

    public Task MouseMove(MouseEventArgs args)
    {
        if (args.OffsetX <= 50 || args.OffsetX >= _width - 50) return Task.CompletedTask;

        double x = args.OffsetX - 50;

        int index = (int) Math.Floor(x / Space);

        PointF p1 = new PointF(index * Space, Items[index].Value);
        PointF p2 = new PointF((index + 1) * Space, Items[index + 1].Value);

        float slope = (p2.Y - p1.Y) / (p2.X - p1.X);

        double y = slope * (x - p1.X) + p1.Y;

        _viewPoint = new PointInfo(x, y);
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        float maxY = (float) Math.Ceiling(Items.Max(item => item.Value));
        float scaleY = GraphHeight / maxY;

        _width = Space * Items.Count;

        _context = await _canvas.CreateCanvas2DAsync();

        await _context.ClearRectAsync(0, 0, _width, Height);

        await _context.SetLineWidthAsync(1);
        await _context.SetStrokeStyleAsync("hotpink");
        await _context.SetFontAsync("12px Montserrat");

        for (int i = 0; i <= GraphHeight / Space; i++)
        {
            await _context.BeginPathAsync();
            await _context.MoveToAsync(50, i * Space + 50);
            await _context.LineToAsync(_width - 50, i * Space + 50);

            string text = (i * Space).ToString();
            TextMetrics? measure = await _context.MeasureTextAsync(text);

            double x = 50 - measure.Width * 0.5f - 25;

            await _context.FillTextAsync(text, x, GraphHeight - i * Space + 50 + 4, 50);

            await _context.StrokeAsync();
        }

        IGraphItem item = Items[0];

        Vector2 p = new Vector2(50, GraphHeight - item.Value * scaleY + 50);

        await _context.SetStrokeStyleAsync("deeppink");
        await _context.MoveToAsync(p.X, p.Y);

        int j = 1;

        do
        {
            item = Items[j];
            p = new Vector2(j * Space + 50, GraphHeight - item.Value * scaleY + 50);

            await _context.LineToAsync(p.X, p.Y);
            j++;
        } while (j < Items.Count);

        await _context.StrokeAsync();

        if (_viewPoint is not null)
        {
            PointInfo pi = _viewPoint.Value;

            await _context.BeginPathAsync();
            await _context.MoveToAsync(pi.X + 50, Height - pi.Y - 50);
            await _context.ArcAsync(pi.X + 50, Height - pi.Y - 50, 3, 0, 360 * Math.PI / 180);
            await _context.FillAsync();
            await _context.FillTextAsync(pi.Y.ToString(CultureInfo.InvariantCulture), pi.X + 50, Height - pi.Y - 60);
        }
    }

    private record struct PointInfo(double X, double Y);
}