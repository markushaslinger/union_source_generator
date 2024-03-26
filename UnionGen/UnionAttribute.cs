using System;
using System.Diagnostics;

namespace UnionGen;

public static class CompileConst
{
    public const string IncludeAttribute = "INCLUDE_UNION_ATTRIBUTE";
}

[Conditional(CompileConst.IncludeAttribute)]
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute<T1, T2> : Attribute
{
    public UnionAttribute(UnionAlignment alignment = UnionAlignment.Unaligned)
    {
        _ = alignment;
    }
}

[Conditional(CompileConst.IncludeAttribute)]
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute<T1, T2, T3> : Attribute
{
    public UnionAttribute(UnionAlignment alignment = UnionAlignment.Unaligned)
    {
        _ = alignment;
    }
}

[Conditional(CompileConst.IncludeAttribute)]
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute<T1, T2, T3, T4> : Attribute
{
    public UnionAttribute(UnionAlignment alignment = UnionAlignment.Unaligned)
    {
        _ = alignment;
    }
}

[Conditional(CompileConst.IncludeAttribute)]
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute<T1, T2, T3, T4, T5> : Attribute
{
    public UnionAttribute(UnionAlignment alignment = UnionAlignment.Unaligned)
    {
        _ = alignment;
    }
}

[Conditional(CompileConst.IncludeAttribute)]
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute<T1, T2, T3, T4, T5, T6> : Attribute
{
    public UnionAttribute(UnionAlignment alignment = UnionAlignment.Unaligned)
    {
        _ = alignment;
    }
}
