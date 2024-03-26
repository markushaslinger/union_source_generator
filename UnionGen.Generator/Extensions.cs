namespace UnionGen;

internal static class Extensions
{
    public static string EnsureTitleCase(this string type)
    {
        Span<char> t = type.ToCharArray();
        t[0] = char.ToUpper(t[0]);
        return t.ToString();
    }
}
