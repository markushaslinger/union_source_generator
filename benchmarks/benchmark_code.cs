using BenchmarkDotNet.Attributes;
using UnionGen;

namespace UnionDemo;

[Union<int[], short>(UnionAlignment.Aligned16)]
public readonly partial struct AlignedShort;
[Union<int[], int>(UnionAlignment.Aligned16)]
public readonly partial struct AlignedInt;
[Union<int[], long>(UnionAlignment.Aligned16)]
public readonly partial struct AlignedLong;
[Union<int[], Foo>(UnionAlignment.Aligned16)]
public readonly partial struct AlignedFoo;
[Union<int, Foo>(UnionAlignment.Aligned16)]
public readonly partial struct AlignedFooNoRef;

[Union<int[], short>(UnionAlignment.Unaligned)]
public readonly partial struct UnalignedShort;
[Union<int[], int>(UnionAlignment.Unaligned)]
public readonly partial struct UnalignedInt;
[Union<int[], long>(UnionAlignment.Unaligned)]
public readonly partial struct UnalignedLong;
[Union<int[], Foo>(UnionAlignment.Unaligned)]
public readonly partial struct UnalignedFoo;
[Union<int, Foo>(UnionAlignment.Unaligned)]
public readonly partial struct UnalignedFooNoRef;


public struct Foo
{
    public short Bar;
    public int Baz;
    public byte Qux;
}

[MemoryDiagnoser(false)]
public class AlignedUnalignedBench
{
    private const int N = 10_000_000;
    private const int PickCount = 1_000_000;
    private readonly AlignedShort[] _alignedShortData;
    private readonly UnalignedShort[] _unalignedShortData;
    private readonly AlignedInt[] _alignedIntData;
    private readonly UnalignedInt[] _unalignedIntData;
    private readonly AlignedLong[] _alignedLongData;
    private readonly UnalignedLong[] _unalignedLongData;
    private readonly AlignedFoo[] _alignedFooData;
    private readonly UnalignedFoo[] _unalignedFooData;
    private readonly AlignedFooNoRef[] _alignedFooNoRefData;
    private readonly UnalignedFooNoRef[] _unalignedFooNoRefData;
    private readonly Random _picker;

    public AlignedUnalignedBench()
    {
        _alignedIntData = new AlignedInt[N];
        _unalignedIntData = new UnalignedInt[N];
        _alignedShortData = new AlignedShort[N];
        _unalignedShortData = new UnalignedShort[N];
        _alignedLongData = new AlignedLong[N];
        _unalignedLongData = new UnalignedLong[N];
        _alignedFooData = new AlignedFoo[N];
        _unalignedFooData = new UnalignedFoo[N];
        _alignedFooNoRefData = new AlignedFooNoRef[N];
        _unalignedFooNoRefData = new UnalignedFooNoRef[N];
        
        var rng = new Random(0xdead);
        for (var i = 0; i < N; i++)
        {
            var val = rng.Next(0, short.MaxValue);
            _alignedIntData[i] = new AlignedInt(val);
            _unalignedIntData[i] = new UnalignedInt(val);
            _alignedShortData[i] = new AlignedShort((short)val);
            _unalignedShortData[i] = new UnalignedShort((short)val);
            _alignedLongData[i] = new AlignedLong(val);
            _unalignedLongData[i] = new UnalignedLong(val);
            _alignedFooData[i] = new AlignedFoo(new Foo { Bar = (short)val, Baz = val, Qux = (byte)val });
            _unalignedFooData[i] = new UnalignedFoo(new Foo { Bar = (short)val, Baz = val, Qux = (byte)val });
            _alignedFooNoRefData[i] = new AlignedFooNoRef(new Foo { Bar = (short)val, Baz = val, Qux = (byte)val });
            _unalignedFooNoRefData[i] = new UnalignedFooNoRef(new Foo { Bar = (short)val, Baz = val, Qux = (byte)val });
        }
        _picker = new Random(0xcafe);
    }

    [Benchmark]
    public long AlignedRandomSumInt()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        { 
            sum += _alignedIntData[_picker.Next(N)].AsInt();
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedRandomSumInt()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        {
            sum += _unalignedIntData[_picker.Next(N)].AsInt();
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedRandomSumShort()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        { 
            sum += _alignedShortData[_picker.Next(N)].AsShort();
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedRandomSumShort()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        {
            sum += _unalignedShortData[_picker.Next(N)].AsShort();
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedRandomSumLong()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        { 
            sum += _alignedLongData[_picker.Next(N)].AsLong();
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedRandomSumLong()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        {
            sum += _unalignedLongData[_picker.Next(N)].AsLong();
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedRandomSumFoo()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        { 
            sum += _alignedFooData[_picker.Next(N)].AsFoo().Baz;
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedRandomSumFoo()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        {
            sum += _unalignedFooData[_picker.Next(N)].AsFoo().Baz;
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedRandomSumFooNoRef()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        { 
            sum += _alignedFooNoRefData[_picker.Next(N)].AsFoo().Baz;
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedRandomSumFooNoRef()
    {
        var sum = 0L;
        for (int i = 0; i < PickCount; i++)
        {
            sum += _unalignedFooNoRefData[_picker.Next(N)].AsFoo().Baz;
        }
        return sum;
    }

    [Benchmark]
    public long AlignedSumInt()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _alignedIntData[i].AsInt();
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedSumInt()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _unalignedIntData[i].AsInt();
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedSumShort()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _alignedShortData[i].AsShort();
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedSumShort()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _unalignedShortData[i].AsShort();
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedSumLong()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _alignedLongData[i].AsLong();
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedSumLong()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _unalignedLongData[i].AsLong();
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedSumFoo()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _alignedFooData[i].AsFoo().Baz;
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedSumFoo()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _unalignedFooData[i].AsFoo().Baz;
        }
        return sum;
    }
    
    [Benchmark]
    public long AlignedSumFooNoRef()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _alignedFooData[i].AsFoo().Baz;
        }
        return sum;
    }

    [Benchmark]
    public long UnalignedSumFooNoRef()
    {
        var sum = 0L;
        for (int i = 0; i < N; i++)
        {
            sum += _unalignedFooData[i].AsFoo().Baz;
        }
        return sum;
    }
}
