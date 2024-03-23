﻿//HintName: UnionGen.DemoObj.g.cs
// <auto-generated by UnionSourceGen />
#nullable enable
namespace Test1
{
    public readonly partial struct DemoObj : IEquatable<DemoObj>
    {
		private readonly byte _index;
		private readonly int _value0;
		private readonly long[] _value1;

		private DemoObj(byte index, int value0 = default!, long[] value1 = default!)
		{
			_index = index;
			_value0 = value0;
			_value1 = value1;
		}

		public DemoObj(int value): this(0, value0: value) {}
		public DemoObj(long[] value): this(1, value1: value) {}
		public DemoObj(): this(-1) {}

		public bool IsInt => _index == 0;
		public bool IsLong[] => _index == 1;

		public int AsInt() =>
			IsInt
				? _value0
				: throw new InvalidOperationException($"Is not of type int but type {GetActualTypeName()}");
		
		public long[] AsLong[]() =>
			IsLong[]
				? _value1
				: throw new InvalidOperationException($"Is not of type long[] but type {GetActualTypeName()}");

		public static implicit operator DemoObj(int value) => new DemoObj(value);
		public static implicit operator DemoObj(long[] value) => new DemoObj(value);
		public static bool operator ==(DemoObj left, DemoObj right) => left.Equals(right);
		public static bool operator !=(DemoObj left, DemoObj right) => !left.Equals(right);

		public TResult Match<TResult>(Func<int, TResult> withInt, Func<long[], TResult> withLong[]) => 		
			_index switch
			{
				0 => withInt(_value0),
				1 => withLong[](_value1),
				_ => throw new InvalidOperationException($"Unknown type index {_index}")
			};

		public void Switch(Action<int> forInt, Action<long[]> forLong[])		
		{
			switch (_index)
			{
				case 0: forInt(_value0); break;
				case 1: forLong[](_value1); break;
				default: throw new InvalidOperationException($"Unknown type index {_index}");
			}
		}

		public override string ToString() => 		
			_index switch
			{
				0 => _value0?.ToString() ?? "null",
				1 => _value1?.ToString() ?? "null",
				_ => throw new InvalidOperationException($"Unknown type index {_index}")
			};

		public bool Equals(DemoObj other) => 
			_index == other._index
				&& _index switch 
				{
					0 => _value0.Equals(other._value0),
					1 => _value1.Equals(other._value1),
					_ => false
				};

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is DemoObj other && Equals(other);
		}

		public override int GetHashCode(){		
			unchecked
			{
				var hash = _index switch
				{
					0 => _value0?.GetHashCode(),
					1 => _value1?.GetHashCode(),
					_ => 0
				} ?? 0;
				return (hash * 397) ^ _index;
			}
		}

		public string GetActualTypeName() =>
			_index switch 
			{
				0 => "int",
				1 => "long[]",
				_ => throw new InvalidOperationException($"Unknown type index {_index}")
			};

    }
}