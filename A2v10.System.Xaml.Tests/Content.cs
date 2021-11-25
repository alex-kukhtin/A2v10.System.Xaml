using A2v10.System.Xaml.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace A2v10.System.Xaml.Tests
{
	[TestClass]
	[TestCategory("Xaml.ContentProperty")]
	public class TestContentProperty
	{
		[TestMethod]
		public void ContentAsObject()
		{
			string xaml = @"
<Alert xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"">
	<Block>
		<Span>I am the span text</Span>
	</Block>
</Alert>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Alert), obj.GetType());
			var p = obj as Alert;
			Assert.IsNotNull(p);	

			var c = p.Content;
			Assert.IsNotNull(c);	
			Assert.AreEqual(typeof(Block), c.GetType());
		}

		[TestMethod]
		public void ContentAsText()
		{
			string xaml = @"
<Alert xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"">
	I am the text
</Alert>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Alert), obj.GetType());
			var p = obj as Alert;
			Assert.IsNotNull(p);

			var c = p.Content;
			Assert.IsNotNull(c);
			Assert.AreEqual(typeof(String), c.GetType());

			Assert.AreEqual("\n\tI am the text\n", p.Content);
		}
	}
}
