// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

// a property element keeps every child or fails on it: plain property - one object, dictionary - one form
[TestClass]
[TestCategory("Xaml.PropertyElement")]
public class PropertyElementChildren
{
    const String MockNs = "clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests";
    const String XNs = "http://schemas.microsoft.com/winfx/2006/xaml";

    static XamlException ParseThrows(String xaml, XamlServicesOptions? options = null)
    {
        return Assert.Throws<XamlException>(() => XamlServices.Parse(xaml, options));
    }

    [TestMethod]
    public void TwoElementsInPlainProperty()
    {
        // was: the first one kept, the second dropped
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}">
                <Page.Toolbar>
                    <Toolbar/>
                    <Button/>
                </Page.Toolbar>
            </Page>
            """);
        Assert.AreEqual("Property element 'Page.Toolbar' takes one object; a second child at line 4, position 10.", ex.Message);
    }

    [TestMethod]
    public void TextThenElementInPlainProperty()
    {
        // was: the text built as a class named ""
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}">
                <Page.Toolbar>
                    Click
                    <Button/>
                </Page.Toolbar>
            </Page>
            """);
        Assert.AreEqual("Property element 'Page.Toolbar' takes one object; a second child at line 4, position 10.", ex.Message);
    }

    [TestMethod]
    public void ElementThenTextInPlainProperty()
    {
        // was: the text dropped
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}">
                <Page.Toolbar>
                    <Toolbar/>tail
                </Page.Toolbar>
            </Page>
            """);
        Assert.AreEqual("Property element 'Page.Toolbar' takes one object; a second child at line 3, position 19.", ex.Message);
    }

    [TestMethod]
    public void HintFromOptionsInPlainProperty()
    {
        var options = new XamlServicesOptions([]) { SingleContentHint = "Wrap the children in <Group>." };
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}">
                <Page.Toolbar>
                    <Toolbar/>
                    <Button/>
                </Page.Toolbar>
            </Page>
            """, options);
        Assert.AreEqual("Property element 'Page.Toolbar' takes one object; a second child at line 4, position 10. Wrap the children in <Group>.", ex.Message);
    }

    [TestMethod]
    public void EmptyPlainPropertyIsNotSet()
    {
        // was: ArgumentOutOfRangeException
        var page = XamlServices.Parse<Page>($"""
            <Page xmlns="{MockNs}">
                <Page.Toolbar/>
            </Page>
            """);
        Assert.IsNull(page.Toolbar);
    }

    [TestMethod]
    public void OneElementInPlainPropertyUnchanged()
    {
        var page = XamlServices.Parse<Page>($"""
            <Page xmlns="{MockNs}">
                <Page.Toolbar>
                    <Toolbar/>
                </Page.Toolbar>
            </Page>
            """);
        Assert.IsInstanceOfType<Toolbar>(page.Toolbar);
    }

    [TestMethod]
    public void MixedTextInCollectionProperty()
    {
        // was: the text built as a class named ""; the same markup as content always worked
        var text = XamlServices.Parse<Text>($"""
            <Text xmlns="{MockNs}"><Text.Inlines>I am the <Span>Bold</Span> text</Text.Inlines></Text>
            """);
        Assert.HasCount(3, text.Inlines);
        Assert.AreEqual("I am the ", text.Inlines[0]);
        Assert.AreEqual("Bold", (text.Inlines[1] as Span)?.Content);
        Assert.AreEqual(" text", text.Inlines[2]);
    }

    [TestMethod]
    public void TextAloneInCollectionProperty()
    {
        // was: the text dropped, Inlines left empty; now converted as Inlines="I am the text" is
        var text = XamlServices.Parse<Text>($"""
            <Text xmlns="{MockNs}"><Text.Inlines>I am the text</Text.Inlines></Text>
            """);
        Assert.HasCount(1, text.Inlines);
        Assert.AreEqual("I am the text", (text.Inlines[0] as Span)?.Content);
    }

    [TestMethod]
    public void TextInTypedCollectionProperty()
    {
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}">
                <Page.Children>
                    text
                    <Span/>
                </Page.Children>
            </Page>
            """);
        Assert.AreEqual("UIElementCollection holds UIElementBase; text does not fit", ex.Message);
    }

    [TestMethod]
    public void TextInTypedCollectionContent()
    {
        // the same check on the content road: was InvalidCastException
        var ex = ParseThrows($"""<Block xmlns="{MockNs}">text<Span/></Block>""");
        Assert.AreEqual("UIElementCollection holds UIElementBase; text does not fit", ex.Message);
    }

    [TestMethod]
    public void TextAmongDictionaryItems()
    {
        // was: "Property 'Key' not found in node ''"
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}" xmlns:x="{XNs}">
                <Page.Resources>
                    <Button x:Key="B1"/>tail
                </Page.Resources>
            </Page>
            """);
        Assert.AreEqual("Property element 'Page.Resources' holds keyed elements; text at line 3, position 29.", ex.Message);
    }

    [TestMethod]
    public void TextAloneInDictionaryProperty()
    {
        // was: the text dropped silently
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}"><Page.Resources>text</Page.Resources></Page>
            """);
        Assert.AreEqual("Invalid value 'text' for property 'Resources'. Expected: ResourceDictionary", ex.Message);
    }

    [TestMethod]
    public void WrapperThenItem()
    {
        // was: the item after the wrapper dropped
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}" xmlns:x="{XNs}">
                <Page.Resources>
                    <ResourceDictionary>
                        <Button x:Key="B1"/>
                    </ResourceDictionary>
                    <Button x:Key="B2"/>
                </Page.Resources>
            </Page>
            """);
        Assert.AreEqual("Property element 'Page.Resources': <ResourceDictionary> must be its only child; a second child at line 6, position 10.", ex.Message);
    }

    [TestMethod]
    public void ItemThenWrapper()
    {
        // was: both forms accepted together
        var ex = ParseThrows($"""
            <Page xmlns="{MockNs}" xmlns:x="{XNs}">
                <Page.Resources>
                    <Button x:Key="B0"/>
                    <ResourceDictionary>
                        <Button x:Key="B1"/>
                    </ResourceDictionary>
                </Page.Resources>
            </Page>
            """);
        Assert.AreEqual("Property element 'Page.Resources': <ResourceDictionary> must be its only child; a second child at line 4, position 10.", ex.Message);
    }

    [TestMethod]
    public void WrapperAloneUnchanged()
    {
        var page = XamlServices.Parse<Page>($"""
            <Page xmlns="{MockNs}" xmlns:x="{XNs}">
                <Page.Resources>
                    <ResourceDictionary>
                        <Button x:Key="B1"/>
                        <Button x:Key="B2"/>
                    </ResourceDictionary>
                </Page.Resources>
            </Page>
            """);
        Assert.AreEqual(2, page.Resources.Count);
        Assert.IsInstanceOfType<Button>(page.Resources["B2"]);
    }
}
