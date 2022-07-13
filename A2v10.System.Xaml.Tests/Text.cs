
using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.Text")]
public class TextInlines
{
	[TestMethod]
	public void InlineCollections()
	{
		string xaml = @"
<Text xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"">
First Segment
<Span>Second Segment</Span>
	Third segment
<Span>Fourth Segment</Span>
</Text>
";
		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Text), obj.GetType());
		var txt = obj as Text;
		Assert.IsNotNull(txt);	
		Assert.AreEqual(4, txt.Inlines.Count);
		Assert.AreEqual("\nFirst Segment\n", txt.Inlines[0]);
		Assert.AreEqual("Second Segment", (txt.Inlines[1] as Span)!.Content);
		Assert.AreEqual("\n\tThird segment\n", txt.Inlines[2]);
		Assert.AreEqual("Fourth Segment", (txt.Inlines[3] as Span)!.Content);
	}
}
