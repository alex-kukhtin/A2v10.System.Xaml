
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
		Assert.HasCount(2, c);

		var c1 = p.Children[0];
		Assert.AreEqual(typeof(Block), c1.GetType());

		var b1 = c1 as Block;
		Assert.IsNotNull(b1);

		Assert.IsTrue(b1.Show);
		Assert.IsFalse(b1.If);
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

    [TestMethod]
    public void DictionaryProperties()
    {
        string xamlContent = """
<Component xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
	<Span x:Key="c1" Content="1"/>
	<Span x:Key="c2" Content="2"/>
</Component>
""";

        string xamlSlots = """
<Component xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"
	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
	<Component.Slots>
		<SlotDictionary>
			<Span x:Key="c1" Content="1"/>
			<Span x:Key="c2" Content="2"/>
		</SlotDictionary>
	</Component.Slots>
</Component>
""";
        
		void __Test(String xaml)
		{
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Component), obj.GetType());
			var c = obj as Component;
			Assert.IsNotNull(c);

			var slots = c.Slots;
			Assert.HasCount(2, slots);

			var c1 = c.Slots["c1"];
			Assert.AreEqual(typeof(Span), c1.GetType());
			Assert.AreEqual("1", (c1 as Span)?.Content);

			var c2 = c.Slots["c2"];
			Assert.AreEqual(typeof(Span), c2.GetType());
			Assert.AreEqual("2", (c2 as Span)?.Content);
		}

		__Test(xamlContent);
        __Test(xamlSlots);
    }

}

