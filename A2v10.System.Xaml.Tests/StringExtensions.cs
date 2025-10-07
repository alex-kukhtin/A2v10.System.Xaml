// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.StringExtensions.Simple")]
public class StringExtensions
{

	[TestMethod]
	public void PascalCase()
	{
		Assert.AreEqual("HelloWorld", "helloWorld".ToPascalCase());
        Assert.AreEqual("Button", "Button".ToPascalCase());
        Assert.AreEqual("Button", "button".ToPascalCase());
        Assert.AreEqual("ButtonElement", "buttonElement".ToPascalCase());
        Assert.AreEqual("ButtonElement", "ButtonElement".ToPascalCase());
        Assert.AreEqual("", (null as String).ToPascalCase());
        Assert.AreEqual("", "".ToPascalCase());
    }
}
