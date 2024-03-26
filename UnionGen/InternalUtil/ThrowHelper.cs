using System;

namespace UnionGen.InternalUtil;

public static class ThrowHelper
{
    public static Exception ThrowUnknownTypeIndex(int indexValue) =>
        new InvalidOperationException($"Unknown type index: {indexValue}");

    public static Exception ThrowNotOfType(string expectedType, string actualType) =>
        new InvalidOperationException($"Is not of requested type {expectedType}, but type {actualType}");
}
