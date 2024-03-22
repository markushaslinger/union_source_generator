using System.Collections;

namespace UnionGen;

internal record struct UnionToGenerate(string Name, string Namespace, ValueEqualityArray<string> TypeParameters);

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