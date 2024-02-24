// Copyright © 2022 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.Extensions.Create")]
public class CreateExtensions
{
	[TestMethod]
	public void Binding()
	{
		var btn = new Button()
		{
			Content = "Text"
		};

		btn.BindImpl.SetBinding("Command", new BindCmd()
		{
			Command = CommandType.Execute, CommandName = "File"
		});

		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);

		var cmd = btn.GetBindingCommand("Command");
		Assert.IsNotNull(cmd);
		Assert.AreEqual(typeof(BindCmd), cmd.GetType());
		Assert.AreEqual("Execute", cmd.Command.ToString());
		Assert.AreEqual("File", cmd.CommandName);
	}

	[TestMethod]
	public void BindingT()
	{
		var btn = BindHelpers.CreateElement<Button>(bi =>
			bi.SetBinding("Command", new BindCmd()
			{
				Command = CommandType.Execute,
				CommandName = "Test"
			})
		);
		btn.Content = "Text";	

		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);

		var cmd = btn.GetBindingCommand("Command");
		Assert.IsNotNull(cmd);
		Assert.AreEqual("Text", btn.Content);
		Assert.AreEqual(typeof(BindCmd), cmd.GetType());
		Assert.AreEqual("Execute", cmd.Command.ToString());
		Assert.AreEqual("Test", cmd.CommandName);
	}


	public static void BindingHelper()
	{

		var obj = BindHelpers.CreateElement(
		new Button()
		{
			Content = "Text"
		},
		new Dictionary<String, BindBase>()
		{
			["Command"] = new BindCmd()
			{
				Command = CommandType.Execute,
				CommandName = "File"
			}
		}
		);

		var btn = obj as Button;
		Assert.IsNotNull(btn);
		Assert.AreEqual("Text", btn.Content);

		var cmd = btn.GetBindingCommand("Command");
		Assert.IsNotNull(cmd);
		Assert.AreEqual(typeof(BindCmd), cmd.GetType());
		Assert.AreEqual("Execute", cmd.Command.ToString());
		Assert.AreEqual("File", cmd.CommandName);
	}
}
