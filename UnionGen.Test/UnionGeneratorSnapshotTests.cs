namespace UnionGen.Test;

public sealed class UnionGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesUnionStructCorrectly()
    {
        // The source code to test
        var source = """
                     namespace Test1;
                     using UnionGen;
                     
                     [ProtoMember(1)]
                     [Union<int,string>]
                     public partial struct DemoObj;
                     """;

        // Pass the source code to our helper and snapshot test the output
        return SnapshotTestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesUnionStructCorrectly_WithArray()
    {
        // The source code to test
        var source = """
                     namespace Test1;
                     using UnionGen;

                     [ProtoMember(1)]
                     [Union<int,long[]>]
                     public partial struct DemoObj;
                     """;

        // Pass the source code to our helper and snapshot test the output
        return SnapshotTestHelper.Verify(source);
    }
}
