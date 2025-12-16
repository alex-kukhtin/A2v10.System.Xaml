
using A2v10.System.Xaml.Tests.Mock;
using A2v10.System.Xaml.Tests.Mock.Drawing;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.ObjectProps")]
public class ObjectProps
{
	[TestMethod]
	public void CardText()
	{
		String xaml = """
        <Page xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" Title="PageTitle">
            <Card>
                <Card.Text>
                    <Text>Text</Text>
                </Card.Text>
            </Card>
        </Page>
        """;
		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Page), obj.GetType());
		var p = obj as Page;
		Assert.IsNotNull(p);

		var c = p.Children;
		Assert.HasCount(1, c);

		var c1 = p.Children[0];
		Assert.AreEqual(typeof(Card), c1.GetType());

		var b1 = c1 as Card;
		Assert.IsNotNull(b1);

		Assert.IsNotNull(b1.Text);
        Assert.AreEqual(typeof(Text), b1.Text.GetType());

		var txt = b1.Text as Text;
		Assert.IsNotNull(txt);
        Assert.AreEqual("Text", (txt.Inlines[0] as Span)!.Content);
    }

    [TestMethod]
    public void CardTextTitleFirst()
    {
        String xaml = """
        <Page Title="PageTitle" xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
            <Card>
                <Card.Text>
                    <Text>Text</Text>
                </Card.Text>
            </Card>
        </Page>
        """;
        var obj = XamlServices.Parse(xaml, null);

        Assert.AreEqual(typeof(Page), obj.GetType());
        var p = obj as Page;
        Assert.IsNotNull(p);

        var c = p.Children;
        Assert.HasCount(1, c);

        var c1 = p.Children[0];
        Assert.AreEqual(typeof(Card), c1.GetType());

        var b1 = c1 as Card;
        Assert.IsNotNull(b1);

        Assert.IsNotNull(b1.Text);
        Assert.AreEqual(typeof(Text), b1.Text.GetType());

        var txt = b1.Text as Text;
        Assert.IsNotNull(txt);
        Assert.AreEqual("Text", (txt.Inlines[0] as Span)!.Content);
    }

    [TestMethod]
    public void CardTextPrefix()
    {
        string xaml = @"
<Page xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" Title=""PageTitle""
    xmlns:d=""clr-namespace:A2v10.System.Xaml.Tests.Mock.Drawing;assembly=A2v10.System.Xaml.Tests""
>
<d:CardDrawing>
	<d:CardDrawing.Text>
		<Text>Text</Text>
	</d:CardDrawing.Text>
</d:CardDrawing>
</Page>
";
        var obj = XamlServices.Parse(xaml, null);

        Assert.AreEqual(typeof(Page), obj.GetType());
        var p = obj as Page;
        Assert.IsNotNull(p);

        var c = p.Children;
        Assert.HasCount(1, c);

        var c1 = p.Children[0];
        Assert.AreEqual(typeof(CardDrawing), c1.GetType());

        var b1 = c1 as CardDrawing;
        Assert.IsNotNull(b1);

        Assert.IsNotNull(b1.Text);
        Assert.AreEqual(typeof(Text), b1.Text.GetType());

        var txt = b1.Text as Text;
        Assert.IsNotNull(txt);
        Assert.AreEqual("Text", (txt.Inlines[0] as Span)!.Content);
    }
}
