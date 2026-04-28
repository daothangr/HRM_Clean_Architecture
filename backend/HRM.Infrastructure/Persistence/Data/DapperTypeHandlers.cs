using Dapper;
using HRM.Application.Common.Utils;
using System.Data;

namespace HRM.Infrastructure.Persistence.Data;

internal static class DapperTypeHandlers
{
    private static bool _registered;
    private static readonly object LockObj = new();

    public static void Register()
    {
        if (_registered)
            return;

        lock (LockObj)
        {
            if (_registered)
                return;

            SqlMapper.AddTypeHandler(new TimeOnlyHandler());
            SqlMapper.AddTypeHandler(new NullableTimeOnlyHandler());
            SqlMapper.AddTypeHandler(new DateOnlyHandler());
            SqlMapper.AddTypeHandler(new NullableDateOnlyHandler());
            _registered = true;
        }
    }

    private sealed class TimeOnlyHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = TemporalUtils.ToTimeSpan(value);
        }

        public override TimeOnly Parse(object value)
        {
            return value switch
            {
                TimeOnly t => t,
                TimeSpan ts => TemporalUtils.ToTimeOnly(ts),
                DateTime dt => TimeOnly.FromDateTime(dt),
                string s when TimeSpan.TryParse(s, out var parsed) => TemporalUtils.ToTimeOnly(parsed),
                _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to TimeOnly")
            };
        }
    }

    private sealed class NullableTimeOnlyHandler : SqlMapper.TypeHandler<TimeOnly?>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly? value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = TemporalUtils.ToTimeSpan(value) ?? (object)DBNull.Value;
        }

        public override TimeOnly? Parse(object value)
        {
            return value switch
            {
                null => null,
                DBNull => null,
                TimeOnly t => t,
                TimeSpan ts => TemporalUtils.ToTimeOnly(ts),
                DateTime dt => TimeOnly.FromDateTime(dt),
                string s when TimeSpan.TryParse(s, out var parsed) => TemporalUtils.ToTimeOnly(parsed),
                _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to TimeOnly?")
            };
        }
    }

    private sealed class DateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = TemporalUtils.ToDateTime(TemporalUtils.NormalizeSqlDate(value));
        }

        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateOnly d => d,
                DateTime dt => TemporalUtils.ToDateOnly(dt),
                DateTimeOffset dto => TemporalUtils.ToDateOnly(dto),
                string s when DateOnly.TryParse(s, out var parsedDateOnly) => parsedDateOnly,
                string s when DateTime.TryParse(s, out var parsedDateTime) => TemporalUtils.ToDateOnly(parsedDateTime),
                _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to DateOnly")
            };
        }
    }

    private sealed class NullableDateOnlyHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            parameter.DbType = DbType.Date;
            var normalized = TemporalUtils.NormalizeSqlDate(value);
            parameter.Value = normalized.HasValue
                ? TemporalUtils.ToDateTime(normalized.Value)
                : (object)DBNull.Value;
        }

        public override DateOnly? Parse(object value)
        {
            return value switch
            {
                null => null,
                DBNull => null,
                DateOnly d => d,
                DateTime dt => TemporalUtils.ToDateOnly(dt),
                DateTimeOffset dto => TemporalUtils.ToDateOnly(dto),
                string s when DateOnly.TryParse(s, out var parsedDateOnly) => parsedDateOnly,
                string s when DateTime.TryParse(s, out var parsedDateTime) => TemporalUtils.ToDateOnly(parsedDateTime),
                _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to DateOnly?")
            };
        }
    }
}
