using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DiegoG.RemoteHud.HudElements;

public enum LortressianDayOfWeek
{
    Sundas,
    Loredas,
    Tirdas,
    Middas,
    Morndas,
    Turdas
}

public class LortressianCalendar : Calendar
{
    protected LortressianCalendar() { }

    public static LortressianCalendar Instance { get; } = new();
    
    public const int DaysInYear = 300;
    public const int MonthsInYear = 10;
    public const int DaysInMonth = 30;
    public const int DaysInWeek = 6;
    
    public override DateTime AddMonths(DateTime time, int months)
        => time += TimeSpan.FromDays(DaysInMonth);

    public override DateTime AddYears(DateTime time, int years)
        => time += TimeSpan.FromDays(DaysInYear);

    public bool IsWorkDay(DateTime time)
        => ((LortressianDayOfWeek)GetDayOfWeek(time)) is LortressianDayOfWeek.Morndas or LortressianDayOfWeek.Turdas;

    public override int GetDayOfMonth(DateTime time)
        => (int)((time.Ticks / TimeSpan.TicksPerDay) % 30);

    public override DayOfWeek GetDayOfWeek(DateTime time)
        => (DayOfWeek)((time.Ticks / TimeSpan.TicksPerDay) % DaysInWeek);

    public override int GetDayOfYear(DateTime time)
        => (int)((time.Ticks / TimeSpan.TicksPerDay) % 300);

    public override int GetDaysInMonth(int year, int month, int era)
        => DaysInMonth;

    public override int GetDaysInYear(int year, int era)
        => DaysInYear;

    public override int GetEra(DateTime time) => 0;

    public override int GetMonth(DateTime time)
        => (int)(((time.Ticks / TimeSpan.TicksPerDay / DaysInMonth) + 1) % MonthsInYear);

    public override int GetMonthsInYear(int year, int era)
        => Months.Length;

    public override int GetYear(DateTime time)
        => (int)((time.Ticks / TimeSpan.TicksPerDay) / DaysInYear);

    public override bool IsLeapDay(int year, int month, int day, int era) => false;
    public override bool IsLeapMonth(int year, int month, int era) => false;
    public override bool IsLeapYear(int year, int era) => false;

    public override DateTime ToDateTime(
        int year, 
        int month, 
        int day, 
        int hour, 
        int minute,
        int second,
        int millisecond,
        int era
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 0);
        ThrowIfNotBetween(month, 1, 10);
        ThrowIfNotBetween(day, 1, 30);
        ThrowIfNotBetween(hour, 0, 23);
        ThrowIfNotBetween(minute, 0, 59);
        ThrowIfNotBetween(second, 0, 59);
        ThrowIfNotBetween(millisecond, 0, 999);
        
        return new DateTime(ticks:
            ((year * (long)DaysInYear
              + (month - 1) * (long)DaysInMonth
              + (day - 1))
             * TimeSpan.TicksPerDay)
            + (hour * TimeSpan.TicksPerHour)
            + (minute * TimeSpan.TicksPerMinute)
            + (second * TimeSpan.TicksPerSecond)
            + (millisecond * TimeSpan.TicksPerMillisecond)
        );

        static void ThrowIfNotBetween(int value, int min, int max, [CallerArgumentExpression(nameof(value))] string? expr = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, min, expr);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, max, expr);
        }
    }

    public override int[] Eras => Array.Empty<int>();
    
    public static ImmutableArray<string> MonthsESP { get; } =
    [
        "Morning Star",
        "Sun's Dawn",
        "First Seed",
        "Rain's Hand",
        "Second Seed",
        "Sun's Height",
        "Last Seed",
        "Frost Fall",
        "Sun's Dusk",
        "Evening Star",
    ];
    
    public static ImmutableArray<string> Months { get; } =
    [
        "Estrella del Alba",
        "Amanecer",
        "Primera Semilla",
        "Llovizna",
        "Segunda Semilla",
        "Zenit del Sol",
        "Ultima Semilla",
        "Helada",
        "Ocaso",
        "Estrella Vespertina"
    ];
}