using System.Collections;

namespace UnionGen
{
    internal sealed record UnionToGenerate(
        string Name,
        string Namespace,
        int RequestedAlignment,
        ValueEqualityArray<TypeParameter> TypeParameters,
        ValueEqualityArray<ParentType> ParentTypes,
        ValueEqualityArray<DiagnosticHelper.Error> Errors)
    {
        public bool AnyReferenceType()
        {
            for (var index = 0; index < TypeParameters.Count; index++)
            {
                var type = TypeParameters[index];
                if (type.IsReferenceType)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal readonly record struct ParentType(string Name, string Type)
    {
        public const string Class = "class";
        public const string Struct = "struct";
        public const string Interface = "interface";
    }

    internal sealed record TypeParameter(string Name, string FullName, bool IsReferenceType)
    {
        private string? _titleCaseName;
        public string TitleCaseName => _titleCaseName ??= Name.EnsureTitleCase();
        public string CallOperator => IsReferenceType
            ? "?."
            : ".";
    }

    internal sealed class ValueEqualityArray<T>(IEnumerable<T> items) : IReadOnlyList<T>, IEquatable<ValueEqualityArray<T>>
        where T : IEquatable<T>
    {
        private readonly T[] _items = items as T[] ?? items.ToArray();

        public T this[int index] => _items[index];

        public int Count => _items.Length;

        public bool Equals(ValueEqualityArray<T>? other)
        {
            if (other is null || Count != other.Count)
            {
                return false;
            }

            for (var i = 0; i < Count; i++)
            {
                if (!this[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as ValueEqualityArray<T>);

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 19;
                foreach (var item in _items)
                {
                    hash = hash * 31 + item.GetHashCode();
                }

                return hash;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}