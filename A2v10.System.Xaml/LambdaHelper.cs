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

			var add = Expression.Lambda<Action<Object, Object>>(
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
			result.AddCollection = TypedItem(add, propType, argType);
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

	/* Every child of a collection goes through here - a content property and a property element
	 * alike - so the item type is checked here once. Text is a child too, and text in a typed
	 * collection (<Block>text<Span/></Block>) was InvalidCastException from the compiled Convert.
	 */
	static Action<Object, Object> TypedItem(Action<Object, Object> add, Type collType, Type itemType)
	{
		return (coll, item) =>
		{
			if (!itemType.IsInstanceOfType(item))
			{
				var what = item is String ? "text" : $"<{item.GetType().Name}>";
				throw new XamlException($"{collType.Name} holds {itemType.Name}; {what} does not fit");
			}
			add(coll, item);
		};
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

