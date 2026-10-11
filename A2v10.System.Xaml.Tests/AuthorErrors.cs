using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

// an error in the markup reaches the caller as XamlException naming the property, the value and the
// expected type - never as a BCL exception (FormatException, ArgumentException, InvalidCastException)
[TestClass]
[TestCategory("Xaml.AuthorErrors")]
public class AuthorErrors
{
    const String MockNs = "clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests";

    static XamlException ParseThrows(String xaml, XamlServicesOptions? options = null)
    {
        return Assert.Throws<XamlException>(() => XamlServices.Parse(xaml, options));
    }

    [TestMethod]
    public void InvalidNumberThrows()
    {
        // was FormatException: Input string was not in a correct format
        var ex = ParseThrows($"""<Element xmlns="{MockNs}" Int32Value="abc"/>""");
        Assert.AreEqual("Invalid value 'abc' for property 'Int32Value'. Expected: Int32", ex.Message);
    }

    [TestMethod]
    public void InvalidNullableNumberThrows()
    {
        var ex = ParseThrows($"""<Element xmlns="{MockNs}" Int32Nullable="1.5"/>""");
        Assert.AreEqual("Invalid value '1.5' for property 'Int32Nullable'. Expected: Int32", ex.Message);
    }

    [TestMethod]
    public void NumberRulesUnchanged()
    {
        // the same NumberStyles as Convert.ChangeType: thousands in a Double are still accepted
        var elem = XamlServices.Parse<Element>($"""<Element xmlns="{MockNs}" DoubleValue="1,000.5" Int32Value=" -7 "/>""");
        Assert.AreEqual(1000.5, elem.DoubleValue);
        Assert.AreEqual(-7, elem.Int32Value);
    }

    [TestMethod]
    public void InvalidEnumNamesPropertyAndValues()
    {
        var ex = ParseThrows($"""<Button xmlns="{MockNs}" Icon="Fiel"/>""");
        Assert.AreEqual("Invalid enum value 'Fiel' for property 'Icon'. Expected: Undefined, File, Folder", ex.Message);
    }

    [TestMethod]
    public void InvalidSystemElementThrows()
    {
        // was FormatException, parsed in the current culture
        var ex = ParseThrows($"""
            <Element xmlns="{MockNs}" xmlns:sys="clr-namespace:System;assembly=mscorlib">
                <Element.Int32Value><sys:Int32>abc</sys:Int32></Element.Int32Value>
            </Element>
            """);
        Assert.AreEqual("Invalid value 'abc' for element 'sys:Int32'. Expected: Int32", ex.Message);
    }

    [TestMethod]
    public void SystemElementInvariantCulture()
    {
        var elem = XamlServices.Parse<Element>($"""
            <Element xmlns="{MockNs}" xmlns:sys="clr-namespace:System;assembly=mscorlib">
                <Element.DoubleValue><sys:Double>2.5</sys:Double></Element.DoubleValue>
            </Element>
            """);
        Assert.AreEqual(2.5, elem.DoubleValue);
    }

    [TestMethod]
    public void ObjectIntoStringPropertyThrows()
    {
        // was InvalidCastException: Object must implement IConvertible
        var ex = ParseThrows($"""
            <Element xmlns="{MockNs}">
                <Element.StringValue><Button/></Element.StringValue>
            </Element>
            """);
        Assert.AreEqual("Invalid value 'A2v10.System.Xaml.Tests.Mock.Button' for property 'StringValue'. Expected: String", ex.Message);
    }

    static String DockXaml(String attached) => $"""
        <Dock xmlns="{MockNs}">
            <Button {attached}/>
        </Dock>
        """;

    static XamlServicesOptions InternalAttached => new([]) { UseInternalAttached = true };

    [TestMethod]
    public void AttachedEnumThrows()
    {
        // was ArgumentException from Enum.Parse
        var ex = ParseThrows(DockXaml("""Dock.Side="Lft" """), InternalAttached);
        Assert.AreEqual("Invalid enum value 'Lft' for property 'Dock.Side'. Expected: Left, Top, Right, Bottom", ex.Message);
    }

    [TestMethod]
    public void AttachedEnumIgnoresCase()
    {
        // as an ordinary enum property does
        var dock = XamlServices.Parse<Dock>(DockXaml("""Dock.Side="top" """), InternalAttached);
        Assert.AreEqual(DockSide.Top, Dock.GetSide(dock.Children[0]));
    }

    [TestMethod]
    public void AttachedNumberThrows()
    {
        // was FormatException from Convert.ChangeType
        var ex = ParseThrows(DockXaml("""Dock.Size="abc" """), InternalAttached);
        Assert.AreEqual("Invalid value 'abc' for property 'Dock.Size'. Expected: Int32", ex.Message);
    }

    [TestMethod]
    public void AttachedNumberRead()
    {
        var dock = XamlServices.Parse<Dock>(DockXaml("""Dock.Size="42" """), InternalAttached);
        Assert.AreEqual(42, Dock.GetSize(dock.Children[0]));
    }

    [TestMethod]
    public void ManagerAttachedNumberThrowsOnRead()
    {
        // the manager keeps the string; the value is converted when the owner reads it (render time)
        var grid = XamlServices.Parse<Grid>($"""
            <Grid xmlns="{MockNs}">
                <Button Grid.Row="abc"/>
            </Grid>
            """);
        var btn = grid.Children[0];
        var ex = Assert.Throws<XamlException>(() => grid.GetRow(btn));
        Assert.AreEqual("Invalid value 'abc' for property 'Grid.Row'. Expected: Int32", ex.Message);
    }

    [TestMethod]
    public void AttributeAndElementThrows()
    {
        // was ArgumentException: An item with the same key has already been added
        var ex = ParseThrows($"""
            <Button xmlns="{MockNs}" Content="a">
                <Button.Content>b</Button.Content>
            </Button>
            """);
        Assert.AreEqual("Property 'Content' is set more than once in 'Button'", ex.Message);
    }

    [TestMethod]
    public void ExtensionAndElementThrows()
    {
        // was silent: both applied, the deferred binding won
        var ex = ParseThrows($$"""
            <Button xmlns="{{MockNs}}" Content="{Bind Name}">
                <Button.Content>b</Button.Content>
            </Button>
            """);
        Assert.AreEqual("Property 'Content' is set more than once in 'Button'", ex.Message);
    }

    [TestMethod]
    public void ElementTwiceThrows()
    {
        var ex = ParseThrows($"""
            <Button xmlns="{MockNs}">
                <Button.Content>a</Button.Content>
                <Button.Content>b</Button.Content>
            </Button>
            """);
        Assert.AreEqual("Property 'Content' is set more than once in 'Button'", ex.Message);
    }

    [TestMethod]
    public void PropertyTwiceInExtensionThrows()
    {
        var ex = ParseThrows($$"""<Button xmlns="{{MockNs}}" Content="{Bind Path=A, Path=B}"/>""");
        Assert.AreEqual("Property 'Path' is set more than once in 'Bind'", ex.Message);
    }

    [TestMethod]
    public void DuplicateKeyThrows()
    {
        // was ArgumentException from Dictionary.Add
        var ex = ParseThrows($"""
            <Component xmlns="{MockNs}" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Span x:Key="c1" Content="1"/>
                <Span x:Key="c1" Content="2"/>
            </Component>
            """);
        Assert.AreEqual("Key 'c1' is used more than once in SlotDictionary", ex.Message);
    }

    [TestMethod]
    public void DuplicateKeyInPropertyElementThrows()
    {
        var ex = ParseThrows($"""
            <Component xmlns="{MockNs}" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Component.Slots>
                    <SlotDictionary>
                        <Span x:Key="c1" Content="1"/>
                        <Span x:Key="c1" Content="2"/>
                    </SlotDictionary>
                </Component.Slots>
            </Component>
            """);
        Assert.AreEqual("Key 'c1' is used more than once in SlotDictionary", ex.Message);
    }
}
