
using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.Collections")]
public class Collections
{
	[TestMethod]
	public void SimpleCollection()
	{
		string xaml = @"
<Page xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" Title=""PageTitle"">
<Block If=""False"" Show=""True"">
	<Button />
	<Span>I am the span text</Span>
</Block>
<Block>
	I am the block text
</Block>
</Page>
";
		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Page), obj.GetType());
		var p = obj as Page;
		Assert.IsNotNull(p);

		var c = p.Children;
		Assert.AreEqual(2, c.Count);

		var c1 = p.Children[0];
		Assert.AreEqual(typeof(Block), c1.GetType());

		var b1 = c1 as Block;
		Assert.IsNotNull(b1);

		Assert.AreEqual(true, b1.Show);
		Assert.AreEqual(false, b1.If);
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
		Assert.AreEqual(2, c.Count);

		var c1 = p.AddOns[0];
		Assert.AreEqual(typeof(Span), c1.GetType());

		var c2 = p.AddOns[1];
		Assert.AreEqual(typeof(Span), c2.GetType());
	}
}

