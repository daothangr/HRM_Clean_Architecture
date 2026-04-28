namespace HRM.Application.Common.Utils;

public static class EnumUtils
{
    public static object ToDbValue(Enum value)
        => Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

    public static object? ToDbValue(object? value)
    {
        if (value is null)
            return null;

        if (value is Enum enumValue)
            return ToDbValue(enumValue);

        return value;
    }

    public static TEnum ParseOrDefault<TEnum>(object? value, TEnum defaultValue = default)
        where TEnum : struct, Enum
    {
        if (value is null)
            return defaultValue;

        if (value is TEnum enumValue)
            return enumValue;

        if (Enum.TryParse<TEnum>(value.ToString(), true, out var parsed))
            return parsed;

        return defaultValue;
    }
}
