﻿using System.Text.RegularExpressions;
using UnionGen;

namespace UnionTest;

public partial interface INested
{
    [Union<int, double, long>]
    public readonly partial struct Nested;
    
    
}

public sealed partial class Foo
{
    [GeneratedRegex("")]
    public static partial Regex Bar();
}
