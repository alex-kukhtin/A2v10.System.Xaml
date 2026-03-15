
namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Extension.Parser")]
public class XamlExtensionParser
{
    [TestMethod]
    public void Parse()
    {
        var xamlSp = new XamlServiceProvider();
        var tdCache = new TypeDescriptorCache();
        var nodeBuilder = new NodeBuilder(xamlSp, tdCache, null);
        nodeBuilder.AddNamespace("", "clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests");

        var ex = Assert.Throws<XamlException>(() =>
        {
            var node = ExtensionParser.Parse(nodeBuilder, "Value");
        });
        Assert.AreEqual("Invalid Xaml extension 'Value'", ex.Message);

        ex = Assert.Throws<XamlException>(() =>
        {
            var node = ExtensionParser.Parse(nodeBuilder, "{Bind Test, Argument={Bind ");
        });

        Assert.StartsWith("Unterminated markup extension in '", ex.Message);
    }
}
