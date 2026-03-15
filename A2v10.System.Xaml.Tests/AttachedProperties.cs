using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.AttachedProperties")]
public class AttachedProperties
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
        var obj = XamlServices.Parse(xaml);

        var parsedGrid = XamlServices.Parse<Grid>(xaml);

        Assert.AreEqual(typeof(Grid), obj.GetType());
        var grid = obj as Grid;
        Assert.AreEqual(typeof(Grid), parsedGrid.GetType());
        Assert.IsNotNull(grid);
        Assert.HasCount(2, grid.Children);
        var btn1 = grid.Children[0] as Button;
        Assert.IsNotNull(btn1);
        Assert.AreEqual("2:3", btn1.Content);
        var apm = grid.AttachedPropertyManager;
        Assert.AreEqual(2, apm.GetProperty<Int32?>("Grid.Row", btn1));
        Assert.AreEqual(3, apm.GetProperty<Int32?>("Grid.Col", btn1));
        Assert.IsNull(apm.GetProperty<Int32?>("Grid.ColSpan", btn1));
        var btn2 = grid.Children[1] as Button;
        Assert.IsNotNull(btn2);
        Assert.AreEqual("1:5", btn2.Content);
        Assert.AreEqual(1, apm.GetProperty<Int32?>("Grid.Row", btn2));
        Assert.AreEqual(5, apm.GetProperty<Int32?>("Grid.Col", btn2));
        Assert.IsNull(apm.GetProperty<Int32?>("Grid.ColSpan", btn2));
    }

    [TestMethod]
    public void Grid2Attached()
    {
        string xaml = """
        <Grid2 xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
            <Button Grid2.Row="2" Grid2.Col="3" Content="2:3"/>
            <Button Grid2.Row="1" Grid2.Col="5" Content="1:5"/>
        </Grid2>
        """;
        var grid = XamlServices.Parse<Grid2>(xaml, 
            new XamlServicesOptions([]) { 
                UseInternalAttached = true
            }
        );


        Assert.IsNotNull(grid);
        Assert.HasCount(2, grid.Children);
        var btn1 = grid.Children[0] as Button;
        Assert.IsNotNull(btn1);
        Assert.AreEqual("2:3", btn1.Content);
        Assert.AreEqual("2", Grid2.GetRow(btn1));
        Assert.AreEqual("3", Grid2.GetCol(btn1));
        var btn2 = grid.Children[1] as Button;
        Assert.IsNotNull(btn2);
        Assert.AreEqual("1:5", btn2.Content);
        Assert.AreEqual("1", Grid2.GetRow(btn2));
        Assert.AreEqual("5", Grid2.GetCol(btn2));
    }
}
