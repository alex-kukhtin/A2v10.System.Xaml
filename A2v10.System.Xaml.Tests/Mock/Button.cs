
using System.ComponentModel;

namespace A2v10.System.Xaml.Tests.Mock;

public enum Icon
{
	Undefined,
	File,
	Folder
}

public class Button :  UIElementBase
{
	public String? Content { get; set; }
	public Object? Command { get; set; }

	public Icon Icon { get; set; }

	public Boolean Block { get; set; }
	public Boolean Italic { get; set; }
	public Boolean Underline { get; set; }
	public Boolean Bold { get; set; }

	private readonly Lazy<UIElementCollection> _addOns = new();

	public UIElementCollection AddOns => _addOns.Value;
}
