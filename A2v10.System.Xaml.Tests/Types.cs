using A2v10.System.Xaml.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.System.Xaml.Tests
{
	[TestClass]
	[TestCategory("Xaml.Types")]
	public class TypesParser
	{
		[TestMethod]
		public void ScalarTypes()
		{
			string xaml = @"
<Element xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	StringValue=""String"" Int32Value=""55"" DoubleValue=""24.3"">
</Element>
";
			var obj = XamlServices.Parse(xaml);

			Assert.AreEqual(typeof(Element), obj.GetType());
			var v = obj as Element;
			Assert.AreEqual("String", v.StringValue);
			Assert.AreEqual(55, v.Int32Value);
			Assert.AreEqual(24.3, v.DoubleValue);
		}

		[TestMethod]
		public void NullableTypes()
		{
			string xaml = @"
<Element xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests""
	StringValue=""String"" Int32Nullable=""55"" DoubleValue=""24.3"">
</Element>
";
			var obj = XamlServices.Parse(xaml);

			Assert.AreEqual(typeof(Element), obj.GetType());
			var v = obj as Element;
			Assert.AreEqual("String", v.StringValue);
			Assert.AreEqual(55, v.Int32Nullable);
			Assert.AreEqual(24.3, v.DoubleValue);
		}

		[TestMethod]
		public void NestedProperies()
		{
			string xaml = @"
<Element xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"">
	<Element.StringValue>String</Element.StringValue>
	<Element.Int32Value>55</Element.Int32Value>
</Element>
";
			var obj = XamlServices.Parse(xaml);

			Assert.AreEqual(typeof(Element), obj.GetType());
			var v = obj as Element;
			Assert.AreEqual("String", v.StringValue);
			Assert.AreEqual(55, v.Int32Value);
		}

		[TestMethod]
		public void NestedSystemTypes()
		{
			string xaml = @"
<Element xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
		xmlns:sys=""clr-namespace:System;assembly=mscorlib"">
	<Element.StringValue>
		<sys:String>String</sys:String>
	</Element.StringValue>
	<Element.Int32Value>
		<sys:Int32>55</sys:Int32>
	</Element.Int32Value>
</Element>
";
			var obj = XamlServices.Parse(xaml);

			Assert.AreEqual(typeof(Element), obj.GetType());
			var v = obj as Element;
			Assert.AreEqual("String", v.StringValue);
			Assert.AreEqual(55, v.Int32Value);
		}
	}
}
