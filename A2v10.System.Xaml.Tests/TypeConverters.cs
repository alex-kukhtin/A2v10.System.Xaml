using A2v10.System.Xaml.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.System.Xaml.Tests
{
	[TestClass]
	[TestCategory("Xaml.TypeConverters")]
	public class TypeConverters
	{

		[TestMethod]
		public void SimplePadding()
		{
			string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Content=""Text"" Padding=""11,22,33,44"">
</Button>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Button), obj.GetType());
			var btn = obj as Button;
			Assert.AreEqual("Text", btn.Content);
			var p = btn.Padding;
			Assert.AreEqual(Length.FromString("11").Value, p.Top.Value);
			Assert.AreEqual(Length.FromString("22").Value, p.Right.Value);
			Assert.AreEqual(Length.FromString("33").Value, p.Bottom.Value);
			Assert.AreEqual(Length.FromString("44").Value, p.Left.Value);

		}

	}
}
