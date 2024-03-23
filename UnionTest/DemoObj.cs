
using MyNamespace;
using UnionGen;
using UnionGen.Types;

namespace UnionTest
{
    [Union<int, double, long[], Foo[], Dictionary<string, bool>>]
    public readonly partial struct DemoObj;

    [Union<Result<int>, NotFound>]
    public readonly partial struct SimpleObj;

}

namespace MyNamespace
{
    public class Foo;
}
