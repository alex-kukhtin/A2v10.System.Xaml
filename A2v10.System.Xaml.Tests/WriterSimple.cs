using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
	[TestMethod]
	public void SimpleWithChildren()
	{
		String xaml = """
<Sequence xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" Ref="Ref0">
    <Code Ref="Ref1">X = X + 1</Code>
    <Code Ref="Ref2">X = X + 2</Code>
    <Code Ref="Ref3">X = X + 3</Code>
</Sequence>
""";

		var obj = new Sequence() { 
			Ref = "Ref0", 
			Activities = [
				new Code() {Ref = "Ref1", Script="X = X + 1" },
                new Code() {Ref = "Ref2", Script="X = X + 2" },
                new Code() {Ref = "Ref3", Script="X = X + 3" },
            ]
        };

		var written = XamlServices.Write(obj);

		Assert.IsTrue(Utils.XmlEquals(written, xaml));
	}

    [TestMethod]
    public void SimpleEmum()
    {
        String xamlDefault = """
        <Variable xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" 
            Name="VarStr">
        </Variable>
        """;

        String xamlNum = """
        <Variable xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" 
            Name="VarNum" Type="Number">
        </Variable>
        """;

        var varString = new Variable() { Name = "VarStr", Type = VariableType.String };
        var varNumber = new Variable() { Name = "VarNum", Type = VariableType.Number };

        var writtenStr = XamlServices.Write(varString);
        var writtenNum = XamlServices.Write(varNumber);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xamlDefault));
        Assert.IsTrue(Utils.XmlEquals(writtenNum, xamlNum));
    }


    [TestMethod]
    public void CardText()
    {
        String xaml = """
        <Page xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" Title="PageTitle">
            <Card Text="Text"/>
        </Page>
        """;

        var page = new Page()
        {
            Title = "PageTitle",
            Children = [
                new Card() {
                    Text = "Text"
                }
            ]
        };
        var writtenStr = XamlServices.Write(page);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }


    [TestMethod]
    public void InlineCollections()
    {
        String xaml = """
        <Text xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
            First Segment
            <Span>Second Segment</Span>
            Third segment
            <Span Show="True">Fourth Segment</Span>
        </Text>
        """;
        var text = new Text()
        {
            Inlines = [
                "First Segment",
                new Span() {
                    Content = "Second Segment"
                },
                "Third segment",
                new Span() {
                    Content = "Fourth Segment", Show = true
                }
            ]
        };
        var writtenStr = XamlServices.Write(text);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));

    }

    [TestMethod]
    public void ContentAsObject()
    {
        String xaml = """
        <Alert xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
            <Block>
                <Span>I am the span text</Span>
            </Block>
        </Alert>
        """;

        var alert = new Alert()
        {
            Content = new Block()
            {
                Children = [
                    new Span() {
                        Content = "I am the span text"
                    }
                ]
            }
        };

        var writtenStr = XamlServices.Write(alert);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }
}
