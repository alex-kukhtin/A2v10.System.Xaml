using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock;

[TypeConverter(typeof(InlineCollectionConverter))]
public class InlineCollection : List<Object>
{

}

[ContentProperty("Inlines")]
public class Text : UIElementBase
{
	public InlineCollection Inlines { get; set; } = new InlineCollection();
}

public class InlineCollectionConverter : TypeConverter
{
	public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		if (sourceType == typeof(String))
			return true;
		else if (sourceType == typeof(InlineCollection))
			return true;
		return false;
	}

	public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
	{
		if (value == null)
			return null;
		if (value is String strVal)
		{
			var coll = new InlineCollection();
			coll.Add(new Span() { Content = strVal });
			return coll;
		}
		throw new XamlException($"Invalid length value '{value}'");
	}
}
