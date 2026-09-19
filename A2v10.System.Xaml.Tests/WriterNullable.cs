// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

// a nullable property uses null for "not set", so its default value
// (false, 0, enum member 0) must survive the write

[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
    [TestMethod]
    public void NullableBooleanFalse()
    {
        String xaml = """
        <Span xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
            Show="False" If="True">Text</Span>
        """;

        var span = new Span()
        {
            Content = "Text",
            Show = false,
            If = true
        };

        var writtenStr = XamlServices.Write(span);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));

        var back = XamlServices.Parse<Span>(writtenStr);
        Assert.AreEqual(false, back.Show);
        Assert.AreEqual(true, back.If);
    }

    [TestMethod]
    public void NullableBooleanNull()
    {
        String xaml = """
        <Span xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests">Text</Span>
        """;

        var span = new Span()
        {
            Content = "Text"
        };

        var writtenStr = XamlServices.Write(span);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));

        var back = XamlServices.Parse<Span>(writtenStr);
        Assert.IsNull(back.Show);
        Assert.IsNull(back.If);
    }

    [TestMethod]
    public void NullableInt32Zero()
    {
        String xaml = """
        <CollectionView xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
            PageSize="0"/>
        """;

        var cw = new CollectionView()
        {
            PageSize = 0
        };

        var writtenStr = XamlServices.Write(cw);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));

        var back = XamlServices.Parse<CollectionView>(writtenStr);
        Assert.AreEqual(0, back.PageSize);
    }

    [TestMethod]
    public void NullableInt32Null()
    {
        String xaml = """
        <CollectionView xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"/>
        """;

        var cw = new CollectionView();

        var writtenStr = XamlServices.Write(cw);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));

        var back = XamlServices.Parse<CollectionView>(writtenStr);
        Assert.IsNull(back.PageSize);
    }

    [TestMethod]
    public void NullableEnumDefaultMember()
    {
        // Type is not nullable - VariableType.String (0) is still omitted
        String xaml = """
        <Variable xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
            Name="Var" TypeNullable="String"/>
        """;

        var variable = new Variable()
        {
            Name = "Var",
            Type = VariableType.String,
            TypeNullable = VariableType.String
        };

        var writtenStr = XamlServices.Write(variable);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));

        var back = XamlServices.Parse<Variable>(writtenStr);
        Assert.AreEqual(VariableType.String, back.TypeNullable);
    }

    [TestMethod]
    public void NullableEnumNull()
    {
        String xaml = """
        <Variable xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
            Name="Var" Type="Number"/>
        """;

        var variable = new Variable()
        {
            Name = "Var",
            Type = VariableType.Number
        };

        var writtenStr = XamlServices.Write(variable);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));

        var back = XamlServices.Parse<Variable>(writtenStr);
        Assert.IsNull(back.TypeNullable);
    }
}
