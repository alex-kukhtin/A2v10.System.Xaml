// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
    [TestMethod]
    public void ResourceDictionary()
    {
        String xaml = """
        <Page xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
                xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" 
            Title="PageTitle">
            <Page.Resources>
                <ResourceDictionary>
                    <Button Content="B1" x:Key="B1" />
                    <Button Content="B2" x:Key="B2" />
                </ResourceDictionary>
            </Page.Resources>
        </Page>
        """;

        var sp = new XamlServiceProvider();

        var b1 = new Button()
        {
            Content = "B1",
        };
        var b2 = new Button()
        {
            Content = "B2",
        };
        var page = new Page()
        {
            Title = "PageTitle",
            Resources = new ResourceDictionary()
            {
                {"B1", b1},
                {"B2", b2}
            }
        };

        var writtenStr = XamlServices.Write(page);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }
    [TestMethod]
    public void Styles()
    {
        var xaml = """
        <Styles xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
        	<Style x:Key="s1">
        		<Setter Property="Bold" Value="True" />
        	</Style>
        	<Style x:Key="s2">
        		<Setter Property="Italic" Value="True" />
        	</Style>
        </Styles>
        """;

        var styles = new Styles()
        {
            {"s1",
                new Style()
                {
                    new Setter() {Property = "Bold", Value = true }
                }
            },
            {"s2",
                new Style()
                {
                    new Setter() {Property = "Italic", Value = true }
                }
            }
        };

        var writtenStr = XamlServices.Write(styles);
        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }
}
