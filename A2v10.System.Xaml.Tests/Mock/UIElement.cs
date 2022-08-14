// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System.ComponentModel;
using System.Globalization;

namespace A2v10.System.Xaml.Tests.Mock;
public class UIElementBase : ISupportBinding, ISupportInitialize
{
	public Boolean? Show { get; set; }
	public Boolean? If { get; set; }
	public Thickness? Padding { get; set; }

	private readonly BindImpl _bindImpl = new();

	#region ISupportBinding
	public BindImpl BindImpl => _bindImpl;

	public Bind? GetBinding(String name)
	{
		return _bindImpl.GetBinding(name);
	}

	public BindCmd? GetBindingCommand(String name)
	{
		return _bindImpl.GetBindingCommand(name);
	}
	#endregion


	# region ISupportInitialize
	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}
	#endregion
}

[TypeConverter(typeof(UICollectionConverter))]
public class UIElementCollection : List<UIElementBase>
{
	public UIElementCollection()
	{

	}
}

public class UICollectionConverter : TypeConverter
{
	public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		if (sourceType == typeof(String))
			return true;
		else if (sourceType == typeof(UIElementBase))
			return true;
		return base.CanConvertFrom(context, sourceType);
	}

	public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
	{
		if (value == null)
			return null;
		if (value is String valStr)
		{
			var x = new UIElementCollection
			{
				new Span() { Content = valStr }
			};
			return x;
		}
		else if (value is UIElementBase uiElemBase)
		{
			var x = new UIElementCollection
			{
				uiElemBase
			};
			return x;
		}
		return base.ConvertFrom(context, culture, value);
	}
}

