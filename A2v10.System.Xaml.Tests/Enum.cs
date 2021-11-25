using A2v10.System.Xaml.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.System.Xaml.Tests
{
	[TestClass]
	[TestCategory("Xaml.Enum")]
	public class EnumParser
	{
		[TestMethod]
		public void DefaultEnum()
		{
			string xaml = @"
<Variable xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Name=""Var0"">
</Variable>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Variable), obj.GetType());
			var v = obj as Variable;
			Assert.IsNotNull(v);	
			Assert.AreEqual("Var0", v.Name);
			Assert.AreEqual(VariableType.String, v.Type);
		}

		[TestMethod]
		public void SimpleEnum()
		{
			string xaml = @"
<Variable xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Name=""Var0"" Type=""String"">
</Variable>
";
			var obj = XamlServices.Parse(xaml, XamlServicesOptions.BpmnXamlOptions);

			Assert.AreEqual(typeof(Variable), obj.GetType());
			var v = obj as Variable;
			Assert.IsNotNull(v);
			Assert.AreEqual("Var0", v.Name);
			Assert.AreEqual(VariableType.String, v.Type);
		}
	}
}
