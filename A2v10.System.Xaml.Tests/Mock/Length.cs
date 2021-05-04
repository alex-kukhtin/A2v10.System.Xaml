using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock
{
	public enum LengthType
	{
		Pixel,
		Percent
	}

	[TypeConverter(typeof(LengthConverter))]
	public class Length
	{
		public String Value;

		public override String ToString()
		{
			return Value;
		}

		public Boolean IsEmpty => String.IsNullOrEmpty(Value);
		public Boolean IsPixel => (Value != null) && Value.EndsWith("px");

		internal static Boolean IsValidLength(String strVal)
		{
			return (strVal.EndsWith("%") ||
					strVal.EndsWith("px") ||
					strVal.EndsWith("vh") ||
					strVal.EndsWith("vw") ||
					strVal.EndsWith("vmin") ||
					strVal.EndsWith("vmax") ||
					strVal.EndsWith("mm") ||
					strVal.EndsWith("cm") ||
					strVal.EndsWith("pt") ||
					strVal.EndsWith("in") ||
					strVal.EndsWith("ch") ||
					strVal.EndsWith("ex") ||
					strVal.EndsWith("em") ||
					strVal.EndsWith("rem"));
		}

		public static Length FromString(String strVal)
		{
			strVal = strVal.Trim();
			if (strVal == "Auto")
				return new Length() { Value = "auto" };
			if (strVal == "Fit")
				return new Length() { Value = "fit-content" };
			else if (strVal == "0")
				return new Length() { Value = strVal };
			else if (strVal.StartsWith("Calc("))
				return new Length() { Value = strVal };
			else if (IsValidLength(strVal))
				return new Length() { Value = strVal };
			else if (Double.TryParse(strVal, out Double dblVal))
				return new Length() { Value = $"{dblVal}px" };
			throw new XamlException($"Invalid length value '{strVal}'");
		}
	}

	public class LengthConverter : TypeConverter
	{
		public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(String))
				return true;
			else if (sourceType == typeof(Length))
				return true;
			return false;
		}

		public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
		{
			if (value == null)
				return null;
			if (value is String)
			{
				String strVal = value.ToString();
				return Length.FromString(strVal);
			}
			throw new XamlException($"Invalid length value '{value}'");
		}
	}
}
