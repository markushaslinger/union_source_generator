using UnionDemo;
using UnionGen.Types;
using UnionTest;

var demo = new DemoObj(4);
Console.WriteLine(demo.IsInt);
Console.WriteLine(demo.AsInt());

var demo2 = new DemoObj([1L, 2L, 3L]);
Console.WriteLine(demo2.IsLongArray);
demo2.Switch(forInt: _ => {},
             forDouble: _ => {},
             forLongArray: a => Console.WriteLine($"{a.GetType()} {a.Length}"),
             forListOfFoo: _ => {},
             forDictionaryOfStringAndBoolean: _ => {}
             );

var simple = new SimpleObj(new Result<int>(12));
var found = simple.IsNotFound;
var result = simple.AsResultOfInt32();

simple.Switch(r => Console.WriteLine($"Found: {r}"),
              _ => Console.WriteLine("not found"));
var res = simple.Match(r => r.Value * 2,
                       _ => -1);

var nested = new INested.Nested(42);

return;

SimpleObj CreateSimple() => new NotFound();


