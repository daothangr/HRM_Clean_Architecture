namespace HRM.Shared.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrTrimmedEmpty(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}
