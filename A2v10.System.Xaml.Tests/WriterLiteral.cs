using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

// writing is the inverse of reading: a literal that starts with '{' must come back as itself,
// not as a markup extension, and a binding must come back as a binding
[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
    static Button ButtonRoundTrip(Button button)
    {
        var written = XamlServices.Write(button);
        var back = XamlServices.Parse(written) as Button;
        Assert.IsNotNull(back);
        return back;
    }

    [TestMethod]
    public void LiteralInBracesRoundTrip()
    {
        // was written as is and read back as an extension
        var button = new Button() { Content = "{x: 1}" };
        Assert.Contains("""Content="{}{x: 1}" """, XamlServices.Write(button));
        Assert.AreEqual("{x: 1}", ButtonRoundTrip(button).Content);
    }

    [TestMethod]
    public void EscapedLiteralRoundTrip()
    {
        // was read back without its first two characters
        var button = new Button() { Content = "{}abc" };
        Assert.AreEqual("{}abc", ButtonRoundTrip(button).Content);
    }

    [TestMethod]
    public void UnclosedBraceLiteralRoundTrip()
    {
        var button = new Button() { Content = "{abc" };
        Assert.AreEqual("{abc", ButtonRoundTrip(button).Content);
    }

    [TestMethod]
    public void PlainLiteralRoundTrip()
    {
        var button = new Button() { Content = "plain" };
        Assert.Contains("""Content="plain" """, XamlServices.Write(button));
        Assert.AreEqual("plain", ButtonRoundTrip(button).Content);
    }

    [TestMethod]
    public void BindingRoundTrip()
    {
        var button = new Button() { Content = "{x}" };
        button.BindImpl.SetBinding("Show", new Bind("Element.Show"));
        Assert.Contains("""Show="{Bind Element.Show}" """, XamlServices.Write(button));

        var back = ButtonRoundTrip(button);
        var bind = back.GetBinding("Show");
        Assert.IsNotNull(bind);
        Assert.AreEqual("Element.Show", bind.Path);
        Assert.AreEqual("{x}", back.Content);
    }

    [TestMethod]
    public void ContentTextNotEscaped()
    {
        // element text is never parsed by the reader, so '{}' would stay in the value
        var span = new Span() { Content = "{x}" };
        var written = XamlServices.Write(span);
        Assert.Contains(">{x}<", written);

        var back = XamlServices.Parse(written) as Span;
        Assert.IsNotNull(back);
        Assert.AreEqual("{x}", back.Content);
    }
}
