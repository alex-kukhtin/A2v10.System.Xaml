using System.Runtime.CompilerServices;

namespace A2v10.System.Xaml.Tests;

// the descriptor cache lives as long as the process (XamlServices keeps a static one), so nothing in it
// may reach a document: TypeDescriptor.BuildNode held the first NodeBuilder, and through it that document
[TestClass]
[TestCategory("Xaml.DescriptorCache")]
public class DescriptorCache
{
    // not inlined: in a Debug build a local of the test method itself may live to the end of the method
    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference ReadRoot(TypeDescriptorCache cache)
    {
        var reader = new XamlReaderService(cache);
        var root = reader.ParseXml("""
            <Button xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
                Content="{Bind Name}"/>
            """);
        return new WeakReference(root);
    }

    [TestMethod]
    public void CacheDoesNotHoldDocument()
    {
        var cache = new TypeDescriptorCache();
        var root = ReadRoot(cache);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(root.IsAlive);
        GC.KeepAlive(cache);
    }
}
