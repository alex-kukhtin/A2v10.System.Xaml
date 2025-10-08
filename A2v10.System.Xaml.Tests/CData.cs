
using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.CData")]
public class CData
{
	[TestMethod]
	public void SimpleCData()
	{
		string xaml = @"
<Page xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" Title=""PageTitle"">
	<Page.Code>
		<![CDATA[
		const expr = (sum) => sum + 5;
		]]>
	</Page.Code>
</Page>
";
		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Page), obj.GetType());
		var p = obj as Page;
		Assert.IsNotNull(p);

		var c = p.Children;
		Assert.HasCount(0, c);

		Assert.AreEqual("PageTitle", p.Title);
		Assert.AreEqual("const expr = (sum) => sum + 5;", p?.Code?.Trim());
	}

	[TestMethod]
	public void AddOns()
	{
		string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"">
<Button.AddOns>
	<Span Content=""1""/>
	<Span Content=""2""/>
</Button.AddOns>
</Button>
";
		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Button), obj.GetType());
		var p = obj as Button;
		Assert.IsNotNull(p);

		var c = p.AddOns;
		Assert.HasCount(2, c);

		var c1 = p.AddOns[0];
		Assert.AreEqual(typeof(Span), c1.GetType());

		var c2 = p.AddOns[1];
		Assert.AreEqual(typeof(Span), c2.GetType());
	}
}

