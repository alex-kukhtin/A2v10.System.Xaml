// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

using System.ComponentModel;
using System.Reflection;

namespace A2v10.System.Xaml;

public record SpecialPropertyDescriptor(String Name);

public record PropertyDescriptor
{
	public PropertyDescriptor(PropertyInfo propInfo)
    {
		PropertyInfo = propInfo;
    }
	public PropertyInfo PropertyInfo { get; }
	public Func<Object>? Constructor { get; init; }
	public Action<Object, Object>? AddMethod { get; init; }
	public Action<Object, String, Object>? AddDictionaryMethod { get; init; }
	public TypeConverter? TypeConverter { get; init; }

	public Boolean IsPlain => AddMethod == null && AddDictionaryMethod == null;
	public Boolean IsArray => AddMethod != null && Constructor != null;
	public Boolean IsDictionary => AddDictionaryMethod != null && Constructor != null;

	/* Two forms of the items, never mixed:
	 *   implicit  <Page.Resources><Button x:Key=../>..</Page.Resources>
	 *   wrapper   <Page.Resources><ResourceDictionary>..</ResourceDictionary></Page.Resources> - XamlWriter emits it
	 * The wrapper is the dictionary object itself, so it is the only child. It used to end the
	 * loop with a return: items before it were added, items after it were lost.
	 */
	void AddToDictionary(NodeBuilder builder, Object dict, XamlNode node)
	{
		if (AddDictionaryMethod == null)
			throw new InvalidProgramException("AddToDictionary failed");
		var children = node.Children.Value;
		var dictName = dict.GetType().Name;
		if (children.Exists(ch => ch.Name == dictName))
		{
			if (children.Count > 1)
				throw new XamlException($"Property element '{node.Name}': <{dictName}> must be its only child; a second child{NodeBuilder.At(children[1])}.");
			if (children[0].HasChildren)
				AddToDictionary(builder, dict, children[0]);
			return;
		}
		foreach (var nd in children)
		{
			if (nd is XamlTextContent)
				throw new XamlException($"Property element '{node.Name}' holds keyed elements; text{NodeBuilder.At(nd)}.");
			if (!nd.Properties.TryGetValue("Key", out var key) || key is not SpecialPropertyDescriptor spec)
				throw new XamlException($"Property 'Key' not found in node '{nd.Name}'");
			var dVal = builder.BuildNode(nd);
			if (dVal != null)
				AddDictionaryMethod(dict, spec.Name, dVal);
		}
	}

	/* Text alone is the value an attribute would give: <Text.Inlines>abc</Text.Inlines> is
	 * Inlines="abc", converted by SetPropertyValue. The collection and dictionary branches used to
	 * return null here, and the text was lost.
	 * Children otherwise, text among them: a text child is a String, as in a content property.
	 */
	public Object? BuildElement(NodeBuilder builder, XamlNode node)
	{
		if (node.IsSimpleContent)
			return node.SimpleContent;
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
			if (Constructor == null)
				throw new InvalidProgramException("BuildElement failed. Constructor not defined");
			var dict = Constructor();

			AddToDictionary(builder, dict, node);

			return dict;
		}
		else if (IsArray)
		{
			if (!node.HasChildren)
				return null;
			if (Constructor == null || AddMethod == null)
				throw new InvalidProgramException("BuildElement failed. Constructor or AddMethod not defined");
			var arr = Constructor();
			foreach (var nd in node.Children.Value)
			{
				var item = builder.BuildChild(nd);
				if (item != null)
					AddMethod(arr, item);
			}
			return arr;
		}
		throw new NotImplementedException($"Property: {PropertyInfo.Name}");
	}
}

