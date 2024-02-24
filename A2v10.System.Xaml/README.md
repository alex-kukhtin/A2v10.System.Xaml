# About

**A2v10.System.Xaml** is a simple lightweight and fast Xaml Reader for A2v10.Platform.


# Related Packages

* [A2v10.ViewEngine.Xaml](https://www.nuget.org/packages/A2v10.ViewEngine.Xaml)
* [A2v10.Workflow](https://www.nuget.org/packages/A2v10.Workflow)
* [A2v10.Xaml.Report](https://www.nuget.org/packages/A2v10.Xaml.Report)

# Tips & Tricks

The escape sequence ({}) is used so that an open brace ({) can be used as a literal character in XAML.

```xml
<Object Property="{} Literal" />

<Object>
	<Object.Property>
	{} Literal
	</Object.Property>
</Object>
```

[See also](https://learn.microsoft.com/en-us/dotnet/desktop/xaml-services/escape-sequence-markup-extension)

# Feedback

A2v10.Platform is released as open source under the MIT license.
Bug reports and contributions are welcome at the GitHub repository.
