# About

**A2v10.System.Xaml** is a simple, lightweight, and fast XAML reader for A2v10.Platform. 
 
 Provides XAML parsing and object graph construction without any dependency 
 on WPF or Windows-specific assemblies, making it suitable 
 for cross-platform server-side and non-UI scenarios.


# Features

* **Lightweight** — zero external dependencies; no WPF or Windows runtime required
* **Fast** — optimized for high-throughput server-side XAML parsing
* **Cross-platform** — targets .NET Standard 2.1 and .NET 6+, runs on Linux, macOS, and Windows
* **Standard-compatible** — mirrors the familiar `System.Xaml` API surface
* **Markup extension support** — handles `{Binding}`, `{StaticResource}`, and custom markup extensions
* **Escape sequence support** — use `{}` to include literal braces in property values

# Quick Start

```csharp
using A2v10.System.Xaml;

// untyped
var obj = XamlServices.Parse(xaml);

// typed — throws if the root element is not MyObject
var myObj = XamlServices.Parse<MyObject>(xaml);

// serialize
var xamlString = XamlServices.Write(obj);
```

# Related Packages

* [A2v10.ViewEngine.Xaml](https://www.nuget.org/packages/A2v10.ViewEngine.Xaml)
* [A2v10.Workflow](https://www.nuget.org/packages/A2v10.Workflow)
* [A2v10.Workflow.Serialization](https://www.nuget.org/packages/A2v10.Workflow.Serialization)
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

**A2v10.System.Xaml** is released as open source under the MIT license.
Bug reports and contributions are welcome at the GitHub repository.
