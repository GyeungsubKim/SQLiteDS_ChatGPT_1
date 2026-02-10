namespace SQLiteDS_ChatGPT_1.Daishin
{
    internal static class ComHelpers
    {
        public static string AsString(object? obj) => obj?.ToString() ?? string.Empty;

        public static long AsLong(object? obj, long fallback = 0L)
        {
            if (obj == null) return fallback;
            if (long.TryParse(obj.ToString(), out var v)) return v;
            if (double.TryParse(obj.ToString(), out var dv)) return (long)dv;
            return fallback;
        }

        public static decimal AsDecimal(object? obj, decimal fallback = 0m)
        {
            if (obj == null) return fallback;
            if (decimal.TryParse(obj.ToString(), out var v)) return v;
            if (double.TryParse(obj.ToString(), out var dv)) return (decimal)dv;
            return fallback;
        }
    }
}
