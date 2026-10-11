// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.ComponentModel;
using System.Globalization;

namespace A2v10.System.Xaml;
public static class PropertyConvertor
{
	public static Object? ConvertValue(Object value, Type type, TypeConverter? typeConverter)
	{
		return ConvertValue(value, type, typeConverter, target: null);
	}

	/* Every value the author writes passes here, and a wrong one leaves as XamlException naming the
	 * target ("property 'Width'", "element 'sys:Int32'"), the value and the expected type - never
	 * as FormatException or InvalidCastException, which name nothing.
	 * Without a catch, the value is checked before it is converted, and the check IS the
	 * conversion: TryParse per TypeCode, with the NumberStyles Convert.ChangeType uses, so what was
	 * read before is read now ("1,000.5" into Double included). TypeConverter.IsValid was not taken:
	 * the standard converters accept another set (DoubleConverter refuses thousands) and IsValid
	 * itself is a try/catch around ConvertFrom.
	 * A custom converter (Length, Thickness) answers for its own errors.
	 */
	internal static Object? ConvertValue(Object value, Type type, TypeConverter? typeConverter, String? target)
	{
		if (value == null)
			return null;

		var valueType = value.GetType();

		type = Nullable.GetUnderlyingType(type) ?? type;

		if (valueType == type || type == typeof(Object))
			return value;

		if (valueType.IsAssignableTo(type))
			return value;

		if (type.IsEnum) {
			if (Enum.TryParse(type, value.ToString()!, ignoreCase: true, out Object? val))
				return val;
			if (target == null)
				throw new XamlException($"Invalid enum value '{value}' for '{type.Name}'");
			throw new XamlException($"Invalid enum value '{value}' for {target}. Expected: {String.Join(", ", Enum.GetNames(type))}");
		}

		if (typeConverter != null)
		{
			if (typeConverter.CanConvertFrom(valueType))
				return typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
		}

		// a built object has no text to parse: it is assignable or it is an error
		var text = value as String ?? (value as IConvertible)?.ToString(CultureInfo.InvariantCulture);
		var result = text != null ? ParseText(text, type) : null;
		if (result != null)
			return result;
		var where = target != null ? $" for {target}" : String.Empty;
		throw new XamlException($"Invalid value '{value}'{where}. Expected: {type.Name}");
	}

	// null - the text is not a value of the type, or the type has no text form
	private static Object? ParseText(String text, Type type)
	{
		var inv = CultureInfo.InvariantCulture;
		const NumberStyles integer = NumberStyles.Integer;
		const NumberStyles floating = NumberStyles.Float | NumberStyles.AllowThousands;
		return Type.GetTypeCode(type) switch
		{
			TypeCode.String => text,
			TypeCode.Boolean => Boolean.TryParse(text, out var v) ? v : null,
			TypeCode.Char => Char.TryParse(text, out var v) ? v : null,
			TypeCode.SByte => SByte.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.Byte => Byte.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.Int16 => Int16.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.UInt16 => UInt16.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.Int32 => Int32.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.UInt32 => UInt32.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.Int64 => Int64.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.UInt64 => UInt64.TryParse(text, integer, inv, out var v) ? v : null,
			TypeCode.Single => Single.TryParse(text, floating, inv, out var v) ? v : null,
			TypeCode.Double => Double.TryParse(text, floating, inv, out var v) ? v : null,
			TypeCode.Decimal => Decimal.TryParse(text, NumberStyles.Number, inv, out var v) ? v : null,
			TypeCode.DateTime => DateTime.TryParse(text, inv, DateTimeStyles.None, out var v) ? v : null,
			_ => null
		};
	}
}
