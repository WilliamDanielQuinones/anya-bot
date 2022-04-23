namespace App;

public static class Helpers
{
    public static List<List<T>> Split<T>(IList<T> source, int chunks)
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunks)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}
