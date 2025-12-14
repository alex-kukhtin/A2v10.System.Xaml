// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
    [TestMethod]
    public void SimpleBinding()
    {
        String xaml = """
        <Button xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
            Show = "{Bind Element.Show}" Content="Simple Bind" Command="{BindCmd Run}" />
        """;

        var b1 = new Button()
        {
            Content = "Simple Bind",
        };
        b1.BindImpl.SetBinding("Show", new Bind("Element.Show"));
        b1.BindImpl.SetBinding("Command", new BindCmd("Run"));

        var writtenStr = XamlServices.Write(b1);

        Assert.IsTrue(Utils.XmlEquals(writtenStr, xaml));
    }

}
