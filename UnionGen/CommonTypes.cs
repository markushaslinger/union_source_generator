using System.Runtime.InteropServices;

namespace UnionGen.Types;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Yes;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct No;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Unknown;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct True;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct False;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct All;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Some;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct None;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Found;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct NotFound;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Failure;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Success;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Irrelevant;

public readonly record struct Success<T>
{
    public Success(T value)
    {
        Value = value;
    }

    public T Value { get; }
}

public readonly record struct Result<T>
{
    public Result(T value)
    {
        Value = value;
    }

    public T Value { get; }
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Error;

public readonly record struct Error<T>
{
    public Error(T value)
    {
        Value = value;
    }

    public T Value { get; }
}
