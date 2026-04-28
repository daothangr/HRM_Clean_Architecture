using HRM.Application.Common.Constants;

namespace HRM.Application.Common.Utils;

public static class TemporalUtils
{
    public static TimeSpan ToTimeSpan(TimeOnly value) => value.ToTimeSpan();

    public static TimeSpan? ToTimeSpan(TimeOnly? value) => value?.ToTimeSpan();

    public static TimeOnly ToTimeOnly(TimeSpan value) => TimeOnly.FromTimeSpan(value);

    public static TimeOnly? ToTimeOnly(TimeSpan? value)
        => value.HasValue ? TimeOnly.FromTimeSpan(value.Value) : null;

    public static DateTime ToDateTime(DateOnly value)
        => value.ToDateTime(TimeOnly.MinValue);

    public static DateTime? ToDateTime(DateOnly? value)
        => value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : null;

    public static DateOnly ToDateOnly(DateTime value)
        => DateOnly.FromDateTime(value);

    public static DateOnly? ToDateOnly(DateTime? value)
        => value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

    public static DateOnly ToDateOnly(DateTimeOffset value)
        => DateOnly.FromDateTime(value.UtcDateTime);

    public static DateOnly? ToDateOnly(DateTimeOffset? value)
        => value.HasValue ? DateOnly.FromDateTime(value.Value.UtcDateTime) : null;

    public static bool IsSqlDateValid(DateOnly value)
        => ToDateTime(value) >= DateTimeConstants.SqlMinDate;

    public static bool IsSqlDateValid(DateTime value)
        => value >= DateTimeConstants.SqlMinDate;

    public static bool IsSqlDateValid(DateTimeOffset value)
        => value.UtcDateTime >= DateTimeConstants.SqlMinDate;

    public static DateTime NormalizeSqlDate(DateTime value, bool dateOnly = false)
    {
        if (!IsSqlDateValid(value))
            return DateTimeConstants.SqlMinDate;

        return dateOnly ? value.Date : value;
    }

    public static DateTime? NormalizeSqlDate(DateTime? value, bool dateOnly = false)
    {
        if (!value.HasValue)
            return null;

        return IsSqlDateValid(value.Value)
            ? (dateOnly ? value.Value.Date : value.Value)
            : null;
    }

    public static DateTimeOffset NormalizeSqlDate(DateTimeOffset value)
    {
        if (!IsSqlDateValid(value))
            return new DateTimeOffset(DateTimeConstants.SqlMinDate, TimeSpan.Zero);

        return value;
    }

    public static DateTimeOffset? NormalizeSqlDate(DateTimeOffset? value)
    {
        if (!value.HasValue)
            return null;

        return IsSqlDateValid(value.Value) ? value.Value : null;
    }

    public static DateOnly NormalizeSqlDate(DateOnly value)
    {
        if (!IsSqlDateValid(value))
            return DateOnly.FromDateTime(DateTimeConstants.SqlMinDate);

        return value;
    }

    public static DateOnly? NormalizeSqlDate(DateOnly? value)
    {
        if (!value.HasValue)
            return null;

        return IsSqlDateValid(value.Value) ? value.Value : null;
    }
}
