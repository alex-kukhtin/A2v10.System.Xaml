// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using System.Xml;

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

// a content property that holds one object fails on a second child instead of losing it
[TestClass]
[TestCategory("Xaml.SingleContent")]
public class SingleContent
{
    const String MockNs = "clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests";

    static XamlException ParseThrows(String xaml, XamlServicesOptions? options = null)
    {
        return Assert.Throws<XamlException>(() => XamlServices.Parse(xaml, options));
    }

    [TestMethod]
    public void TwoElementsInObjectContent()
    {
        // was: the last one kept
        var ex = ParseThrows($"""
            <Alert xmlns="{MockNs}">
                <Span/>
                <Button/>
            </Alert>
            """);
        Assert.AreEqual("Element 'Alert': content property 'Content' takes one object; a second child at line 3, position 6.", ex.Message);
    }

    [TestMethod]
    public void TwoElementsInTypedContent()
    {
        // was: the first one kept, the second dropped; the element is named as written
        var ex = ParseThrows($"""
            <m:Frame xmlns:m="{MockNs}">
                <m:Span/>
                <m:Button/>
            </m:Frame>
            """);
        Assert.AreEqual("Element 'm:Frame': content property 'Body' takes one object; a second child at line 3, position 6.", ex.Message);
    }

    [TestMethod]
    public void TextThenElementInObjectContent()
    {
        var ex = ParseThrows($"""
            <Alert xmlns="{MockNs}">
                Click
                <Span>here</Span>
            </Alert>
            """);
        Assert.AreEqual("Element 'Alert': content property 'Content' takes one object; a second child at line 3, position 6.", ex.Message);
    }

    [TestMethod]
    public void ElementThenTextInObjectContent()
    {
        // the position of a text child is where the text starts
        var ex = ParseThrows($"""
            <Alert xmlns="{MockNs}">
                <Span/>tail
            </Alert>
            """);
        Assert.AreEqual("Element 'Alert': content property 'Content' takes one object; a second child at line 2, position 12.", ex.Message);
    }

    [TestMethod]
    public void NoPositionWithoutLineInfo()
    {
        // XmlNodeReader has no IXmlLineInfo
        var doc = new XmlDocument();
        doc.LoadXml($"""<Alert xmlns="{MockNs}"><Span/><Button/></Alert>""");
        var ex = Assert.Throws<XamlException>(() => XamlServices.Load(new XmlNodeReader(doc)));
        Assert.AreEqual("Element 'Alert': content property 'Content' takes one object; a second child.", ex.Message);
    }

    [TestMethod]
    public void HintFromOptions()
    {
        var options = new XamlServicesOptions([]) { SingleContentHint = "Wrap the children in <Group>: it renders no element of its own." };
        var ex = ParseThrows($"""
            <Frame xmlns="{MockNs}">
                <Span/>
                <Button/>
            </Frame>
            """, options);
        Assert.AreEqual("Element 'Frame': content property 'Body' takes one object; a second child at line 3, position 6. Wrap the children in <Group>: it renders no element of its own.", ex.Message);
    }

    [TestMethod]
    public void OneChildUnchanged()
    {
        var alert = XamlServices.Parse<Alert>($"""
            <Alert xmlns="{MockNs}">
                <Span/>
            </Alert>
            """);
        Assert.IsInstanceOfType<Span>(alert.Content);

        var frame = XamlServices.Parse<Frame>($"""
            <Frame xmlns="{MockNs}">
                <Button/>
            </Frame>
            """);
        Assert.IsInstanceOfType<Button>(frame.Body);
    }

    [TestMethod]
    public void CollectionContentUnchanged()
    {
        var block = XamlServices.Parse<Block>($"""
            <Block xmlns="{MockNs}">
                <Span/>
                <Button/>
            </Block>
            """);
        Assert.AreEqual(2, block.Children.Count);
    }
}
