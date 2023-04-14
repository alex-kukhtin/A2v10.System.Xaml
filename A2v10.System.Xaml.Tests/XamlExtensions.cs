// Copyright © 2022 Alex Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

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
		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);
		Assert.AreEqual(Icon.File, btn.Icon);
	}

	[TestMethod]
	public void EscapeBraces()
	{
		string xaml = @"
<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Content=""Text"" Command=""{}{Text in braces}"">
</Button>
";
		var obj = XamlServices.Parse(xaml, null);

		Assert.AreEqual(typeof(Button), obj.GetType());
		var btn = obj as Button;
		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);
		Assert.AreEqual("{Text in braces}", btn.Command);
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
		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);

		var cmd = btn.GetBindingCommand("Command");
		Assert.IsNotNull(cmd);
		Assert.AreEqual(typeof(BindCmd), cmd.GetType());
		Assert.AreEqual("Execute", cmd.Command.ToString());
		Assert.AreEqual("File", cmd.CommandName);
	}

	[TestMethod]
	public void BindingCodeGen()
	{
		var btn = new Button()
		{
			Content = "Text",
		};
		(btn as ISupportBinding).BindImpl.SetBinding("Command",
			new BindCmd() { Command = CommandType.Execute, CommandName = "File" });


		/*
		string xaml = @"
		<Button xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
			Content=""Text"" Command=""{BindCmd Execute, CommandName='File'}"">
		</Button>";
		var obj = XamlServices.Parse(xaml, null);
		Assert.AreEqual(typeof(Button), obj.GetType());
		*/

		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);

		var cmd = btn.GetBindingCommand("Command");
		Assert.IsNotNull(cmd);
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
		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);

		var icon = btn.GetBinding("Icon");
		Assert.IsNotNull(icon);
		Assert.AreEqual(typeof(Bind), icon.GetType());
		Assert.AreEqual("Icon", icon.Path);

		var cmd = btn.GetBindingCommand("Command");
		Assert.IsNotNull(cmd);
		Assert.AreEqual(typeof(BindCmd), cmd.GetType());
		Assert.AreEqual("Execute", cmd.Command.ToString());
		Assert.AreEqual("File", cmd.CommandName);

		var arg = cmd.GetBinding("Argument");
		Assert.IsNotNull(arg);
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
		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);

		var cmd = btn.GetBindingCommand("Command");
		Assert.IsNotNull(cmd);
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
		Assert.IsNotNull(page);
		var tb = page.Toolbar as Button;
		Assert.IsNotNull(tb);
		Assert.AreEqual(typeof(Button), tb.GetType());
		var bind = tb.GetBinding("Content");
		Assert.IsNotNull(bind);
		Assert.AreEqual("Parent.Pager", bind.Path);
	}
}
