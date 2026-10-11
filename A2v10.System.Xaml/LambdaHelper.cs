// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace A2v10.System.Xaml;
public record CollectionLambdas
{
	public Action<Object, Object>? AddCollection;
	public Action<Object, String, Object>? AddDictionary;
}

public static class LambdaHelper
{
	public static CollectionLambdas AddCollectionMethods(Type propType)
	{
		var result = new CollectionLambdas();

		// an instance Add only: the static Decimal.Add(Decimal, Decimal) passed for a dictionary Add,
		// and the descriptor of any type with a Decimal property failed to build
		var mtdAdd = propType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
		if (mtdAdd == null)
			return result;
		var inst = Expression.Parameter(typeof(Object));
		var argPrm = Expression.Parameter(typeof(Object));

		var args = mtdAdd.GetParameters();
		if (args.Length == 1)
		{
			var argType = args[0].ParameterType;

			result.AddCollection = Expression.Lambda<Action<Object, Object>?>(
				Expression.Call(
					Expression.Convert(inst, propType),
					mtdAdd,
					Expression.Convert(
						argPrm,
						argType
					)
				),
				inst,
				argPrm
			).Compile();
		} 
		else if (args.Length == 2)
		{
			// .Add(key, value);
			var keyType = args[0].ParameterType;
			var argType = args[1].ParameterType; // value
			var argKey = Expression.Parameter(typeof(Object));

			var add = Expression.Lambda<Action<Object, Object, Object>>(
				Expression.Call(
					Expression.Convert(inst, propType),
					mtdAdd,
					Expression.Convert(
						argKey,
						keyType
					),
					Expression.Convert(
						argPrm,
						argType
					)
				),
				inst,
				argKey,
				argPrm
			).Compile();
			result.AddDictionary = KeyFromText(add, propType, keyType);
		}
		return result;
	}

	/* x:Key is always text, and the key of the dictionary may be anything: the text is converted
	 * as a property value is (PropertyConvertor), so a bad key is XamlException naming the
	 * dictionary. The call used to be built with a String key whatever Add took, and a
	 * Dictionary<UInt32, Row> failed the descriptor of its owner - used in the markup or not.
	 * The repeat is checked on the converted key: "1" and "01" are one key of UInt32. It names the
	 * text as written, which is what the author searches for.
	 */
	static Action<Object, String, Object> KeyFromText(Action<Object, Object, Object> add, Type dictType, Type keyType)
	{
		var isDictionary = typeof(IDictionary).IsAssignableFrom(dictType);
		return (dict, keyText, value) =>
		{
			var key = PropertyConvertor.ConvertValue(keyText, keyType, null, $"x:Key of {dictType.Name}")!;
			if (isDictionary && ((IDictionary)dict).Contains(key))
				throw new XamlException($"Key '{keyText}' is used more than once in {dictType.Name}");
			add(dict, key, value);
		};
	}
}

