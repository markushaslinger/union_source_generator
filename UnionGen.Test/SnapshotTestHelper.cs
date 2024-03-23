using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace UnionGen.Test;

public static class SnapshotTestHelper
{
    public static Task Verify(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        IEnumerable<PortableExecutableReference> references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };
        var compilation = CSharpCompilation.Create(
                                                   assemblyName: "Tests",
                                                   syntaxTrees: new[] { syntaxTree },
                                                   references: references);
        
        var generator = new UnionSourceGen();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        
        return Verifier
            .Verify(driver)
            .UseDirectory("Snapshots");
    }
}
