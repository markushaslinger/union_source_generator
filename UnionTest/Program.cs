namespace UnionTest;

public class Program
{
    public static void Main(string[] args)
    {
        var demo = new DemoObj(4);
        Console.WriteLine(demo.IsFoo);
        Console.WriteLine(demo.GetHashCode());
    }
}