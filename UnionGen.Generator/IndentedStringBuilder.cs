using System.Text;

namespace UnionGen;

internal sealed class IndentedStringBuilder
{
    public const string NewLine = "\r\n";
    private readonly int _allLinesIndent;
    private readonly StringBuilder _builder;

    public IndentedStringBuilder(int allLinesIndent, string? initialContent = null)
    {
        _allLinesIndent = allLinesIndent;
        _builder = string.IsNullOrWhiteSpace(initialContent)
            ? new StringBuilder()
            : new StringBuilder($"{Indent(allLinesIndent)}{initialContent}");
    }

    public void Append(string value)
    {
        _builder.Append(value);
    }
    
    public void Append(StringBuilder otherBuilder)
    {
        _builder.Append(otherBuilder);
    }
    
    public void Append(IndentedStringBuilder otherBuilder)
    {
        _builder.Append(otherBuilder._builder);
    }

    public void AppendLine(string value, int extraIndent = 0)
    {
        _builder.Append(Indent(_allLinesIndent + extraIndent));
        _builder.AppendLine(value);
    }

    public override string ToString() => _builder.ToString();

    private static string Indent(int count)
    {
        const string Indent = "\t";

        return count switch
               {
                   0 => string.Empty,
                   1 => Indent,
                   2 => $"{Indent}{Indent}",
                   3 => $"{Indent}{Indent}{Indent}",
                   4 => $"{Indent}{Indent}{Indent}{Indent}",
                   5 => $"{Indent}{Indent}{Indent}{Indent}{Indent}",
                   6 => $"{Indent}{Indent}{Indent}{Indent}{Indent}{Indent}",
                   _ => string.Concat(Enumerable.Repeat(Indent, count))
               };
    }
}
