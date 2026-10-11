// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

using System.Collections;
using System.Linq.Expressions;

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

		var mtdAdd = propType.GetMethod("Add");
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
			var argType = args[1].ParameterType; // value
			var argKey = Expression.Parameter(typeof(String));

			var add = Expression.Lambda<Action<Object, String, Object>>(
				Expression.Call(
					Expression.Convert(inst, propType),
					mtdAdd,
					argKey,
					Expression.Convert(
						argPrm,
						argType
					)
				),
				inst,
				argKey,
				argPrm
			).Compile();
			result.AddDictionary = UniqueKey(add, propType);
		}
		return result;
	}

	// two elements with one x:Key: Dictionary.Add threw ArgumentException, naming no dictionary
	static Action<Object, String, Object> UniqueKey(Action<Object, String, Object> add, Type dictType)
	{
		if (!typeof(IDictionary).IsAssignableFrom(dictType))
			return add;
		return (dict, key, value) =>
		{
			if (((IDictionary)dict).Contains(key))
				throw new XamlException($"Key '{key}' is used more than once in {dictType.Name}");
			add(dict, key, value);
		};
	}
}

