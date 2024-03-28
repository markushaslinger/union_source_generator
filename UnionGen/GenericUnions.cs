using System;

namespace UnionGen;

public readonly record struct Union<T0, T1>
{
    private readonly T0 _value0;
    private readonly T1 _value1;
    private readonly byte _index;

    public Union(T0 value)
    {
        _value0 = value;
        _value1 = default!;
        _index = 0;
    }
    
    public Union(T1 value)
    {
        _value0 = default!;
        _value1 = value;
        _index = 1;
    }

    [Obsolete(InternalUtil.UnionGenInternalConst.DefaultConstructorWarning, true)]
    public Union()
    {
        _value0 = default!;
        _value1 = default!;
    }
    
    public static implicit operator Union<T0, T1>(T0 value) => new(value);
    public static implicit operator Union<T0, T1>(T1 value) => new(value);
    
    public TResult Match<TResult>(Func<T0, TResult> withFirst, Func<T1, TResult> withSecond) => 		
        _index switch
        {
            0 => withFirst(_value0),
            1 => withSecond(_value1),
            _ => throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index)
        };
    
    public void Switch(Action<T0> forFirst, Action<T1> forSecond)		
    {
        switch (_index)
        {
            case 0: forFirst(_value0); break;
            case 1: forSecond(_value1); break;
            default: throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index);
        }
    }
    
    public override string? ToString() => 
        _index switch
        {
            0 => _value0?.ToString(),
            1 => _value1?.ToString(),
            _ => throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index)
        };
}

public readonly record struct Union<T0, T1, T2>
{
    private readonly T0 _value0;
    private readonly T1 _value1;
    private readonly T2 _value2;
    private readonly byte _index;

    public Union(T0 value)
    {
        _value0 = value;
        _value1 = default!;
        _value2 = default!;
        _index = 0;
    }
    
    public Union(T1 value)
    {
        _value0 = default!;
        _value1 = value;
        _value2 = default!;
        _index = 1;
    }
    
    public Union(T2 value)
    {
        _value0 = default!;
        _value1 = default!;
        _value2 = value;
        _index = 2;
    }

    [Obsolete(InternalUtil.UnionGenInternalConst.DefaultConstructorWarning, true)]
    public Union()
    {
        _value0 = default!;
        _value1 = default!;
        _value2 = default!;
    }
    
    public static implicit operator Union<T0, T1, T2>(T0 value) => new(value);
    public static implicit operator Union<T0, T1, T2>(T1 value) => new(value);
    public static implicit operator Union<T0, T1, T2>(T2 value) => new(value);
    
    public TResult Match<TResult>(Func<T0, TResult> withFirst, 
                                  Func<T1, TResult> withSecond,
                                  Func<T2, TResult> withThird) => 		
        _index switch
        {
            0 => withFirst(_value0),
            1 => withSecond(_value1),
            2 => withThird(_value2),
            _ => throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index)
        };
    
    public void Switch(Action<T0> forFirst, 
                       Action<T1> forSecond,
                       Action<T2> forThird)		
    {
        switch (_index)
        {
            case 0: forFirst(_value0); break;
            case 1: forSecond(_value1); break;
            case 2: forThird(_value2); break;
            default: throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index);
        }
    }
    
    public override string? ToString() => 
        _index switch
        {
            0 => _value0?.ToString(),
            1 => _value1?.ToString(),
            2 => _value2?.ToString(),
            _ => throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index)
        };
}

public readonly record struct Union<T0, T1, T2, T3>
{
    private readonly T0 _value0;
    private readonly T1 _value1;
    private readonly T2 _value2;
    private readonly T3 _value3;
    private readonly byte _index;

    public Union(T0 value)
    {
        _value0 = value;
        _value1 = default!;
        _value2 = default!;
        _value3 = default!;
        _index = 0;
    }
    
    public Union(T1 value)
    {
        _value0 = default!;
        _value1 = value;
        _value2 = default!;
        _value3 = default!;
        _index = 1;
    }
    
    public Union(T2 value)
    {
        _value0 = default!;
        _value1 = default!;
        _value2 = value;
        _value3 = default!;
        _index = 2;
    }
    
    public Union(T3 value)
    {
        _value0 = default!;
        _value1 = default!;
        _value2 = default!;
        _value3 = value;
        _index = 3;
    }

    [Obsolete(InternalUtil.UnionGenInternalConst.DefaultConstructorWarning, true)]
    public Union()
    {
        _value0 = default!;
        _value1 = default!;
        _value2 = default!;
        _value3 = default!;
    }
    
    public static implicit operator Union<T0, T1, T2, T3>(T0 value) => new(value);
    public static implicit operator Union<T0, T1, T2, T3>(T1 value) => new(value);
    public static implicit operator Union<T0, T1, T2, T3>(T2 value) => new(value);
    public static implicit operator Union<T0, T1, T2, T3>(T3 value) => new(value);
    
    public TResult Match<TResult>(Func<T0, TResult> withFirst, 
                                  Func<T1, TResult> withSecond,
                                  Func<T2, TResult> withThird,
                                  Func<T3, TResult> withFourth) => 		
        _index switch
        {
            0 => withFirst(_value0),
            1 => withSecond(_value1),
            2 => withThird(_value2),
            3 => withFourth(_value3),
            _ => throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index)
        };
    
    public void Switch(Action<T0> forFirst, 
                       Action<T1> forSecond,
                       Action<T2> forThird,
                       Action<T3> forFourth)		
    {
        switch (_index)
        {
            case 0: forFirst(_value0); break;
            case 1: forSecond(_value1); break;
            case 2: forThird(_value2); break;
            case 3: forFourth(_value3); break;
            default: throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index);
        }
    }
    
    public override string? ToString() => 
        _index switch
        {
            0 => _value0?.ToString(),
            1 => _value1?.ToString(),
            2 => _value2?.ToString(),
            3 => _value3?.ToString(),
            _ => throw InternalUtil.ExceptionHelper.ThrowUnknownTypeIndex(_index)
        };
}