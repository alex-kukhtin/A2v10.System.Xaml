using A2v10.System.Xaml.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace A2v10.System.Xaml.Tests
{
	[TestClass]
	[TestCategory("Xaml.Resources")]
	public class TestResources
	{
		[TestMethod]
		public void SimpleResource()
		{
			string xaml = @"
<Page xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
	xmlns:sys = ""clr-namespace:System;assembly=mscorlib"">
	<Page.Resources>
		<sys:String x:Key=""StrVal"">StringValue</sys:String>
		<Button x:Key=""Button"" Content=""Click""/>
	</Page.Resources>
</Page>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Page), obj.GetType());
			var p = obj as Page;

			var strVal = p.Resources["StrVal"];
			Assert.AreEqual("StringValue", strVal);
			var btn = p.Resources["Button"] as Button;
			Assert.AreEqual("Click", btn.Content);
		}
	}
}
