namespace WaxComponents.Utils;

public static class NumberExtensions
{
    public static int Percentage(this float value, float max, float min = 0) =>
        (int) ((value - min) / (max - min) * 100);
}