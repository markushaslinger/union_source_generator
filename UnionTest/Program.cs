using UnionGen.Types;
using UnionTest;

var demo = new DemoObj(4);
Console.WriteLine(demo.IsInt);
Console.WriteLine(demo.AsInt());

var simple = CreateSimple();
simple.Switch(
              r =>  Console.WriteLine($"Found: {r}"),
              _ => Console.WriteLine("not found"));

var result = simple.Match(r => r.Value * 2,
                          _ => -1);
Console.WriteLine(result);

return;

SimpleObj CreateSimple() => new NotFound();
