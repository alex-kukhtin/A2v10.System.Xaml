// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests.Mock;

public enum Icon
{
	Undefined,
	File,
	Folder
}

[ContentProperty("Content")]
[ContentAsXamlAttr]
public class Button :  UIElementBase
{
	public Object? Content { get; set; }
	public Object? Command { get; set; }

	public Icon Icon { get; set; }

	public Boolean Block { get; set; }
	public Boolean Italic { get; set; }
	public Boolean Underline { get; set; }
	public Boolean Bold { get; set; }
    public Int32 TabIndex { get; set; }

    private readonly Lazy<UIElementCollection> _addOns = new();

	public UIElementCollection AddOns => _addOns.Value;
}
