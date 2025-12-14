// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
    [TestMethod]
    public void GridAttached()
    {
        String xaml = """
        <Grid xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
            <Button Grid.Row="2" Grid.Col="3" Content="2:3"/>
            <Button Grid.Row="1" Grid.Col="5" Content="1:5"/>
        </Grid>
        """;

        var sp = new XamlServiceProvider();

        var b1 = new Button()
        {
            Content = "2:3",
        };
        var b2 = new Button()
        {
            Content = "1:5",
        };
        var grid = new Grid(sp)
        {
            Children = [b1, b2]
        };
        grid.SetRow(b1, 2);
        grid.SetCol(b1, 3);
        grid.SetRow(b2, 1);
        grid.SetCol(b2, 5);

        var writtenStr = XamlServices.Write(grid);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }

    [TestMethod]
    public void GridNestedAttached()
    {
        String xaml = """
        <Page xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" Title="Page Title">
            <Page.Toolbar>
                <Toolbar Padding="1rem, 2rem">
                    <Button Icon="File" Content="Reload"/>
                </Toolbar>
            </Page.Toolbar>
            <Grid Overflow="Auto">
                <Button Grid.Row="2" Grid.Col="3" Content="2:3"/>
                <Button Grid.Row="1" Grid.Col="5" Content="1:5"/>
            </Grid>
        </Page>
        """;

        var sp = new XamlServiceProvider();

        var b1 = new Button()
        {
            Content = "2:3",
        };
        var b2 = new Button()
        {
            Content = "1:5",
        };
        var grid = new Grid(sp)
        {
            Children = [b1, b2],
            Overflow = Overflow.Auto
        };
        var page = new Page()
        {
            Title = "Page Title",
            Toolbar = new Toolbar()
            {
                Padding = Thickness.FromString("1rem,2rem"),
                Children = [
                    new Button() {
                        Content = "Reload",
                        Icon = Icon.File
                    }
                ]
            },
            Children = [grid]
        };
           
        grid.SetRow(b1, 2);
        grid.SetCol(b1, 3);
        grid.SetRow(b2, 1);
        grid.SetCol(b2, 5);

        var writtenStr = XamlServices.Write(page);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }

    [TestMethod]
    public void GridRowDefinitions()
    {
        String xaml = """
        <Grid xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
                Rows="Auto, 12rem">
            <Button Grid.Row="2" Grid.Col="3" Content="2:3"/>
            <Button Grid.Row="1" Grid.Col="5" Content="1:5"/>
        </Grid>
        """;

        var sp = new XamlServiceProvider();

        var b1 = new Button()
        {
            Content = "2:3",
        };
        var b2 = new Button()
        {
            Content = "1:5",
        };
        var grid = new Grid(sp)
        {
            Children = [b1, b2]
        };
        grid.SetRow(b1, 2);
        grid.SetCol(b1, 3);
        grid.SetRow(b2, 1);
        grid.SetCol(b2, 5);

        grid.Rows = new RowDefinitions()
        {
            new RowDefinition() {Height = Length.FromString("Auto")  },
            new RowDefinition() {Height = Length.FromString("12rem") },
        };

        var writtenStr = XamlServices.Write(grid);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }
}
