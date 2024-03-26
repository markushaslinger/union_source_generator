namespace UnionGen.InternalUtil;

public static class PointerSizeGuard
{
    public static void EnsureAlignment(int alignment)
    {
        if (System.IntPtr.Size > alignment)
        {
            throw new System.NotSupportedException($"Pointer size {System.IntPtr.Size} is not supported by union source generator - expected {alignment} at most.");
        }
    }
}