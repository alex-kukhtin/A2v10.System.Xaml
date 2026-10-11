using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

// x:Key is always text; a dictionary keyed by anything else gets it converted to its key type,
// and the writer writes such a key so that the reader converts it back
[TestClass]
[TestCategory("Xaml.DictionaryKeys")]
public class DictionaryKeys
{
    const String Ns = """xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" """;

    static String KeyedXaml(String numbered) => $"""
        <Keyed {Ns}>
            <Keyed.Numbered>
                {numbered}
            </Keyed.Numbered>
        </Keyed>
        """;

    [TestMethod]
    public void OwnerOfNonStringKeyDictionaryReads()
    {
        // was ArgumentException while building the descriptor of Keyed: a String key passed to Add(UInt32, Span),
        // even with the property not used in the markup
        var keyed = XamlServices.Parse<Keyed>($"""<Keyed {Ns}/>""");
        Assert.IsEmpty(keyed.Numbered);
    }

    [TestMethod]
    public void NumberKeys()
    {
        var keyed = XamlServices.Parse<Keyed>(KeyedXaml("""
            <Span x:Key="1" Content="one"/>
            <Span x:Key="2" Content="two"/>
            """));
        Assert.AreEqual("one", keyed.Numbered[1u].Content);
        Assert.AreEqual("two", keyed.Numbered[2u].Content);
    }

    [TestMethod]
    public void EnumKeys()
    {
        var keyed = XamlServices.Parse<Keyed>($"""
            <Keyed {Ns}>
                <Keyed.Sides>
                    <Span x:Key="Left" Content="L"/>
                </Keyed.Sides>
            </Keyed>
            """);
        Assert.AreEqual("L", keyed.Sides[DockSide.Left].Content);
    }

    [TestMethod]
    public void InvalidKeyThrows()
    {
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(KeyedXaml("""<Span x:Key="abc"/>""")));
        Assert.AreEqual("Invalid value 'abc' for x:Key of NumberedItems. Expected: UInt32", ex.Message);
    }

    [TestMethod]
    public void SameKeyInOtherSpellingThrows()
    {
        // compared after the conversion: "01" is the key 1 once more
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(KeyedXaml("""
            <Span x:Key="1"/>
            <Span x:Key="01"/>
            """)));
        Assert.AreEqual("Key '01' is used more than once in NumberedItems", ex.Message);
    }

    [TestMethod]
    public void WriteNumberKeys()
    {
        // was an element per KeyValuePair`2: only Dictionary<String, Object> was understood in a property
        var keyed = new Keyed();
        keyed.Numbered.Add(1, new Span() { Content = "one" });
        var written = XamlServices.Write(keyed);
        Assert.Contains("Key=\"1\"", written);

        var back = XamlServices.Parse<Keyed>(written);
        Assert.AreEqual("one", back.Numbered[1u].Content);
    }

    [TestMethod]
    public void WriteStringKeysOfTypedValues()
    {
        // Dictionary<String, UIElementBase> is not Dictionary<String, Object> either
        var component = new Component();
        component.Slots.Add("c1", new Span() { Content = "1" });
        var written = XamlServices.Write(component);

        var back = XamlServices.Parse<Component>(written);
        Assert.AreEqual("1", (back.Slots["c1"] as Span)?.Content);
    }

    [TestMethod]
    public void WriteDoubleKeyInvariant()
    {
        // was key.ToString() in the current culture: "1,5", read back as 15
        var rated = new RatedItems() { { 1.5, new Span() { Content = "x" } } };
        var written = XamlServices.Write(rated);
        Assert.Contains("Key=\"1.5\"", written);

        var back = XamlServices.Parse<RatedItems>(written);
        Assert.AreEqual("x", back[1.5].Content);
    }
}
