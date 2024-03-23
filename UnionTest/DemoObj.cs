
using MyNamespace;
using UnionGen;

namespace UnionTest
{
    [Union<int, double, long[], Foo[], Dictionary<string, bool>>]
    public readonly partial struct DemoObj;

}

namespace MyNamespace
{
    public class Foo;
}
