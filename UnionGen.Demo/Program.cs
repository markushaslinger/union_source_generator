using UnionGen.Types;
using UnionDemo;

var demo = new DemoObj(4);
Console.WriteLine(demo.IsInt);
Console.WriteLine(demo.AsInt());

var simple = new SimpleObj(new Result<int>(12));
var found = simple.IsNotFound;
var result = simple.AsResultOfInt32();

simple.Switch(r => Console.WriteLine($"Found: {r}"),
              _ => Console.WriteLine("not found"));
var res = simple.Match(r => r.Value * 2,
                       _ => -1);

return;

SimpleObj CreateSimple() => new NotFound();
