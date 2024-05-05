namespace UnionGen;

internal static class WellKnownTypes
{
    public static string AdjustIfWellKnown(string typeName)
    {
        return typeName switch
               {
                   nameof(Int16)   => "Short",
                   nameof(Int32)   => "Int",
                   nameof(Int64)   => "Long",
                   nameof(UInt16)  => "UShort",
                   nameof(UInt32)  => "UInt",
                   nameof(UInt64)  => "ULong",
                   nameof(Single)  => "Float",
                   nameof(Boolean) => "Bool",
                   nameof(IntPtr)  => "NInt",
                   nameof(UIntPtr) => "NUInt",
                   _               => typeName
               };
    }
}
