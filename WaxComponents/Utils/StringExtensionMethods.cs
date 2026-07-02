namespace WaxComponents.Utils;

public static class StringExtensionMethods
{
    public static string Merge(this string className, params string[] classes)
        => string.Join(" ", new[] { className }.Concat(classes).Where(c => !string.IsNullOrWhiteSpace(c)));
}