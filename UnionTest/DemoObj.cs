
using UnionGen;

namespace UnionTest;

[Union<int, double, float, Foo, bool>]
public readonly partial struct DemoObj;

public class Foo;