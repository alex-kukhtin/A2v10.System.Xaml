using A2v10.System.Xaml.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.System.Xaml.Tests
{
	[TestClass]
	[TestCategory("Xaml.Extensions.Simple")]
	public class ReadExtensions
	{

		[TestMethod]
		public void Simple()
		{
			string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Content=""Text"" Icon=""File"">
</Button>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Button), obj.GetType());
			var btn = obj as Button;
			Assert.AreEqual("Text", btn.Content);
			Assert.AreEqual(Icon.File, btn.Icon);
		}

		[TestMethod]
		public void Binding()
		{
			string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Content=""Text"" Command=""{BindCmd Execute, CommandName='File'}"">
</Button>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Button), obj.GetType());
			var btn = obj as Button;
			Assert.AreEqual("Text", btn.Content);

			var cmd = btn.GetBindingCommand("Command");
			Assert.AreEqual(typeof(BindCmd), cmd.GetType());
			Assert.AreEqual("Execute", cmd.Command.ToString());
			Assert.AreEqual("File", cmd.CommandName);
		}

		[TestMethod]
		public void Full()
		{
			string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Content=""Text"" Icon=""{Bind Icon}"" Command=""{BindCmd Execute, CommandName='File', Argument={Bind Path='Text'}}"">
</Button>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Button), obj.GetType());
			var btn = obj as Button;
			Assert.AreEqual("Text", btn.Content);

			var icon = btn.GetBinding("Icon");
			Assert.AreEqual(typeof(Bind), icon.GetType());
			Assert.AreEqual("Icon", icon.Path);

			var cmd = btn.GetBindingCommand("Command");
			Assert.AreEqual(typeof(BindCmd), cmd.GetType());
			Assert.AreEqual("Execute", cmd.Command.ToString());
			Assert.AreEqual("File", cmd.CommandName);

			var arg = cmd.GetBinding("Argument");
			Assert.AreEqual(typeof(Bind), arg.GetType());
			Assert.AreEqual("Text", arg.Path);
		}

		[TestMethod]
		public void NestedObjectBinding()
		{
			string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Content=""Text"">
	<Button.Command>
		<BindCmd Command=""Execute"" CommandName=""File""/>
	</Button.Command>
</Button>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Button), obj.GetType());
			var btn = obj as Button;
			Assert.AreEqual("Text", btn.Content);

			var cmd = btn.GetBindingCommand("Command");
			Assert.AreEqual(typeof(BindCmd), cmd.GetType());
			Assert.AreEqual("Execute", cmd.Command.ToString());
			Assert.AreEqual("File", cmd.CommandName);
		}


		[TestMethod]
		public void QualifiedProperty()
		{
			string xaml = @"
<Page xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"">
	<Page.Toolbar>
		<Button Content=""{Bind Parent.Pager}""/>
	</Page.Toolbar>
</Page>
";
			var obj = XamlServices.Parse(xaml, null);

			Assert.AreEqual(typeof(Page), obj.GetType());
			var page = obj as Page;
			var tb = page.Toolbar as Button;
			Assert.AreEqual(typeof(Button), tb.GetType());
			var bind = tb.GetBinding("Content");
			Assert.AreEqual("Parent.Pager", bind.Path);
		}
	}
}
