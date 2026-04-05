
using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.Namespaces")]
public class XmlNamespace
{
	[TestMethod]
	public void XmlSpace ()
	{
		string xaml = """
		<Page xmlns="clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests" Title="PageTitle">
			<Span xml:space="preserve">  TEST  </Span>
			<Span>TEST  </Span>
			<Span>
			TEXT
			CONTENT
			TEXT
			</Span>
		</Page>
		""";

		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Page), obj.GetType());
		var p = obj as Page;
		Assert.IsNotNull(p);

		var c = p.Children;
		Assert.HasCount(3, c);
		Assert.IsInstanceOfType<Span>(c[0]);
        Assert.IsInstanceOfType<Span>(c[1]);
        Assert.IsInstanceOfType<Span>(c[2]);
        var sp = c[0] as Span;
		Assert.AreEqual("  TEST  ", sp?.Content);
        sp = c[1] as Span;
        Assert.AreEqual("TEST  ", sp?.Content);

        sp = c[2] as Span;
        Assert.AreEqual("\n\tTEXT\n\tCONTENT\n\tTEXT\n\t", sp?.Content);

        Assert.AreEqual("PageTitle", p.Title);
	}
}

