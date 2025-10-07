using A2v10.System.Xaml.Tests.Mock;

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
			Assert.IsNotNull(btn);
			Assert.AreEqual("Text", btn.Content);
			var p = btn.Padding;
			Assert.IsNotNull(p);
			Assert.AreEqual(Length.FromString("11").Value, p.Top.Value);
			Assert.AreEqual(Length.FromString("22").Value, p.Right.Value);
			Assert.AreEqual(Length.FromString("33").Value, p.Bottom.Value);
			Assert.AreEqual(Length.FromString("44").Value, p.Left.Value);

		}

		[TestMethod]
		public void SimpleBoolean()
		{
			string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Bold=""True"" Italic=""true"" Underline=""False"" Block=""false"">
</Button>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Button), obj.GetType());
			var btn = obj as Button;
			Assert.IsNotNull(btn);
			Assert.AreEqual(true, btn.Bold);
			Assert.AreEqual(true, btn.Italic);
			Assert.AreEqual(false, btn.Underline);
			Assert.AreEqual(false, btn.Block);

		}
	}
}
