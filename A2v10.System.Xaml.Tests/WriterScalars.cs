using A2v10.System.Xaml.Tests.Mock;
using A2v10.System.Xaml.Tests.Mock.Other;

namespace A2v10.System.Xaml.Tests;

// every IConvertible value is an attribute in the invariant culture, read back by the reader as
// the same value; what the writer cannot write readably fails at write time
[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
    static Element ElementRoundTrip(Element element)
    {
        var written = XamlServices.Write(element);
        return XamlServices.Parse<Element>(written);
    }

    [TestMethod]
    public void DoubleRoundTrip()
    {
        // was <Element.DoubleValue><Double xmlns=...></Element.DoubleValue>: no value, unreadable
        foreach (var value in new Double[] { 24.3, 0.1 + 0.2, 1E+20, -0.5 })
            Assert.AreEqual(value, ElementRoundTrip(new Element() { DoubleValue = value }).DoubleValue);
        Assert.Contains("""DoubleValue="24.3" """, XamlServices.Write(new Element() { DoubleValue = 24.3 }));
    }

    [TestMethod]
    public void DecimalAndInt64RoundTrip()
    {
        var back = ElementRoundTrip(new Element() { DecimalValue = 1234.5678M, Int64Value = Int64.MaxValue });
        Assert.AreEqual(1234.5678M, back.DecimalValue);
        Assert.AreEqual(Int64.MaxValue, back.Int64Value);
    }

    [TestMethod]
    public void DateTimeRoundTrip()
    {
        var unspecified = new DateTime(2026, 10, 11, 14, 30, 15).AddTicks(1234567);
        var back = ElementRoundTrip(new Element() { DateValue = unspecified });
        Assert.AreEqual(unspecified, back.DateValue);
        Assert.AreEqual(DateTimeKind.Unspecified, back.DateValue.Kind);

        var utc = new DateTime(2026, 10, 11, 14, 30, 15, DateTimeKind.Utc).AddTicks(1234567);
        back = ElementRoundTrip(new Element() { DateValue = utc });
        Assert.AreEqual(utc, back.DateValue);
        Assert.AreEqual(DateTimeKind.Utc, back.DateValue.Kind);
    }

    [TestMethod]
    public void CharBraceRoundTrip()
    {
        Assert.AreEqual('{', ElementRoundTrip(new Element() { CharValue = '{' }).CharValue);
    }

    [TestMethod]
    public void NullableZeroWritten()
    {
        var element = new Element() { DoubleNullable = 0 };
        Assert.Contains("""DoubleNullable="0" """, XamlServices.Write(element));
        Assert.AreEqual(0, ElementRoundTrip(element).DoubleNullable);
    }

    [TestMethod]
    public void DefaultScalarsNotWritten()
    {
        String xaml = """
        <Element xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"/>
        """;
        Assert.IsTrue(Utils.XmlEquals(XamlServices.Write(new Element()), xaml));
    }

    [TestMethod]
    public void ValueTypeWithoutConverterThrows()
    {
        // was <Shape.Origin><Offset xmlns=.../></Shape.Origin>: X lost, and the reader cannot build a struct
        var ex = Assert.Throws<XamlException>(() => XamlServices.Write(new Shape() { Origin = new Offset() { X = 1 } }));
        Assert.AreEqual("Cannot write property 'Shape.Origin': Offset has no text form (no IXamlConverter)", ex.Message);
        // the default is not written, as 0 of an Int32 is not
        XamlServices.Write(new Shape());
    }

    [TestMethod]
    public void ForeignNamespaceThrows()
    {
        // was an element with a redefined xmlns, which the reader ignores
        var ex = Assert.Throws<XamlException>(() => XamlServices.Write(new Button() { Content = new Foreign() }));
        Assert.AreEqual("Cannot write Foreign in property 'Button.Content': namespace 'clr-namespace:A2v10.System.Xaml.Tests.Mock.Other;assembly=A2v10.System.Xaml.Tests' is not the document namespace 'clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests'", ex.Message);
    }

    [TestMethod]
    public void ForeignNamespaceInCollectionThrows()
    {
        var dock = new Dock() { Children = [new Foreign()] };
        var ex = Assert.Throws<XamlException>(() => XamlServices.Write(dock));
        Assert.StartsWith("Cannot write Foreign in property 'Dock.Children'", ex.Message);
    }
}
