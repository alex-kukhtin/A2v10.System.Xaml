// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.Derived")]
public class Derived
{
	[TestMethod]
	public void BaseProperty()
	{
		string xaml = @"
<Page xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" Title=""PageTitle"">
<Selector Text=""Text"">
   <Control.Button>
		<Button Content=""Button""/>
   </Control.Button>
</Selector>
</Page>
";
		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Page), obj.GetType());
		var p = obj as Page;
		Assert.IsNotNull(p);

		var c = p.Children;
		Assert.HasCount(1, c);

		var c1 = p.Children[0];
		Assert.AreEqual(typeof(Selector), c1.GetType());

		var sel = c1 as Selector;
		Assert.IsNotNull(sel);

		Assert.AreEqual("Text", sel.Text);
		var b1 = sel.Button;
		Assert.IsNotNull(b1);
		Assert.AreEqual("Button", b1.Content);
	}
}

