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

    // Grid keeps values in the manager, Grid2 in static setters - one owner per mode
    static String GridXaml(String attached) => $"""
        <Grid xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
            <Button {attached} Content="x"/>
        </Grid>
        """;

    static String Grid2Xaml(String attached) => $"""
        <Grid2 xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">
            <Button {attached} Content="x"/>
        </Grid2>
        """;

    static XamlServicesOptions InternalAttached => new([]) { UseInternalAttached = true };

    [TestMethod]
    public void UnknownOwnerThrows()
    {
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(GridXaml("""Gird.Row="1" """)));
        Assert.AreEqual("Attached property 'Gird.Row': class Gird not found", ex.Message);
        ex = Assert.Throws<XamlException>(() => XamlServices.Parse(Grid2Xaml("""Gird2.Row="1" """), InternalAttached));
        Assert.AreEqual("Attached property 'Gird2.Row': class Gird2 not found", ex.Message);
    }

    [TestMethod]
    public void UnknownNameThrows()
    {
        // the manager kept it under 'Grid.Rwo', Grid reads 'Grid.Row': lost silently
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(GridXaml("""Grid.Rwo="1" """)));
        Assert.AreEqual("Attached property 'Grid.Rwo' not found in type A2v10.System.Xaml.Tests.Mock.Grid. Declared: Col, Row", ex.Message);
        ex = Assert.Throws<XamlException>(() => XamlServices.Parse(Grid2Xaml("""Grid2.Rwo="1" """), InternalAttached));
        Assert.AreEqual("Attached property 'Grid2.Rwo' not found in type A2v10.System.Xaml.Tests.Mock.Grid2. Declared: Col, Row", ex.Message);
    }

    [TestMethod]
    public void NameIsCaseSensitive()
    {
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(GridXaml("""Grid.ROw="1" """)));
        Assert.Contains("'Grid.ROw'", ex.Message);
    }

    [TestMethod]
    public void OwnerWithoutAttachedPropertiesThrows()
    {
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(GridXaml("""Button.Row="1" """)));
        Assert.AreEqual("Attached property 'Button.Row' not found in type A2v10.System.Xaml.Tests.Mock.Button. The type declares no attached properties", ex.Message);
    }

    [TestMethod]
    public void ThreePartNameThrows()
    {
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(GridXaml("""Grid.Row.X="1" """)));
        Assert.AreEqual("Invalid attached property name 'Grid.Row.X'", ex.Message);
        ex = Assert.Throws<XamlException>(() => XamlServices.Parse(Grid2Xaml("""Grid2.Row.X="1" """), InternalAttached));
        Assert.AreEqual("Invalid attached property name 'Grid2.Row.X'", ex.Message);
    }

    [TestMethod]
    public void SkipUnknownPropertiesSkipsUnknownName()
    {
        var options = new XamlServicesOptions([]) { SkipUnknownProperties = true };
        var grid = XamlServices.Parse<Grid>(GridXaml("""Grid.Rwo="1" Grid.Col="2" """), options);
        var btn = grid.Children[0] as Button;
        Assert.IsNotNull(btn);
        Assert.IsNull(grid.GetRow(btn));
        Assert.AreEqual(2, grid.GetCol(btn));
    }

    [TestMethod]
    public void SkipUnknownPropertiesKeepsUnknownOwnerError()
    {
        var options = new XamlServicesOptions([]) { SkipUnknownProperties = true };
        Assert.Throws<XamlException>(() => XamlServices.Parse(GridXaml("""Gird.Row="1" """), options));
    }

    [TestMethod]
    public void DescriptorDoesNotDependOnMode()
    {
        // one cache, the descriptor of Grid2 built by a manager-mode reader first:
        // it had no setters, and the internal-mode reader dropped the value silently
        var cache = new TypeDescriptorCache();
        var managerReader = new XamlReaderService(cache);
        managerReader.ParseXml(Grid2Xaml("""Grid2.Row="7" """));

        var internalReader = new XamlReaderService(cache) { Options = InternalAttached };
        var grid = internalReader.ParseXml<Grid2>(Grid2Xaml("""Grid2.Row="8" """));
        var btn = grid.Children[0] as Button;
        Assert.IsNotNull(btn);
        Assert.AreEqual("8", Grid2.GetRow(btn));
    }

    [TestMethod]
    public void InternalModeWithoutStaticSetterThrows()
    {
        // Grid is a manager-mode owner: it declares Row but has no static SetRow
        var ex = Assert.Throws<XamlException>(() => XamlServices.Parse(Grid2Xaml("""Grid.Row="1" """), InternalAttached));
        Assert.AreEqual("Attached property 'Grid.Row' has no static setter SetRow in type A2v10.System.Xaml.Tests.Mock.Grid", ex.Message);
    }
}
