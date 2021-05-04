// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace A2v10.System.Xaml
{
	public record SpecialPropertyDescriptor(String Name);

	public record PropertyDescriptor
	{
		public PropertyInfo PropertyInfo { get; init; }
		public Type Type { get; init; }
		public Func<Object> Constructor { get; init; }
		public Action<Object, Object> AddMethod { get; init; }
		public Action<Object, String, Object> AddDictionaryMethod { get; init; }
		public TypeConverter TypeConverter { get; init; }

		public Boolean IsPlain => AddMethod == null && AddDictionaryMethod == null;
		public Boolean IsArray => AddMethod != null && Constructor != null;
		public Boolean IsDictionary => AddDictionaryMethod != null && Constructor != null;

		void AddToDictionary(NodeBuilder builder, Object dict, IEnumerable<XamlNode> children)
		{
			foreach (var nd in children)
			{
				if (nd.Name == dict.GetType().Name)
				{
					if (nd.HasChildren)
						AddToDictionary(builder, dict, nd.Children.Value);
					return;
				}
				else
				{
					var key = nd.Properties["Key"];
					if (key is SpecialPropertyDescriptor spec)
					{
						var dVal = builder.BuildNode(nd);
						if (dVal != null)
							AddDictionaryMethod(dict, spec.Name, dVal);
					}
				}
			}
		}

		public Object BuildElement(NodeBuilder builder, XamlNode node)
		{
			if (IsPlain)
			{
				if (PropertyInfo.CanWrite)
					return XamlNode.GetNodeValue(builder, node);
				else
					throw new XamlException($"Property {PropertyInfo.PropertyType} is read only");
			}
			else if (IsDictionary)
			{
				if (!node.HasChildren)
					return null;
				var dict = Constructor();

				AddToDictionary(builder, dict, node.Children.Value);

				return dict;
			}
			else if (IsArray)
			{
				if (!node.HasChildren)
					return null;
				var arr = Constructor();
				foreach (var nd in node.Children.Value)
					AddMethod(arr, builder.BuildNode(nd));
				return arr;
			}
			throw new NotImplementedException($"Property: {PropertyInfo.Name}");
		}
	}
}
