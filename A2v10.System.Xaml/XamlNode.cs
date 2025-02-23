﻿// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

using System.Linq;
using System.Reflection;

namespace A2v10.System.Xaml;

public record XamlExtensionElem(PropertyInfo PropertyInfo, MarkupExtension Element);
public record XamlAttachedElem(String Name, Object Value);

public class XamlNode(String name)
{
	public String Name { get; } = name;
	public String? ConstructorArgument => _ctorArgument;

	private String? _ctorArgument;

	public Lazy<List<XamlNode>> Children = new();
	public readonly Dictionary<String, Object> Properties = [];
	public readonly List<XamlExtensionElem> Extensions = [];
	public readonly Lazy<List<XamlAttachedElem>> AttachedProperties = new();

	public Boolean HasChildren => Children.IsValueCreated && Children.Value.Count > 0 && !IsSimpleContent;
	public Boolean HasExtensions => Extensions != null && Extensions.Count > 0;

	public Boolean IsSimpleContent => Children.IsValueCreated && Children.Value.Count == 1 && Children.Value[0] is XamlTextContent;

	public String? SimpleContent
	{
		get
		{
			if (Children.Value.Count == 1 && Children.Value[0] is XamlTextContent xamlContent)
				return xamlContent.Text;
			throw new XamlException("Invalid element for simple content");
		}
	}

	public String XKeyName
	{
		get
		{
			if (Properties.Where(x => x.Key == "Key").FirstOrDefault().Value is not SpecialPropertyDescriptor keyProp)
				throw new XamlException($"Property Key not found in type {Name}");
			return keyProp.Name;
		}
	}

	public void SetContent(String text)
	{
		Children.Value.Add(new XamlTextContent() { Text = text });
	}

	public void AddChildren(XamlNode node, NodeBuilder builder)
	{
		if (node.Name.Contains('.'))
		{
			// inner or attached
			var parts = node.Name.Split(".");
			if (parts.Length != 2)
				throw new XamlException($"Invalid attribute name '{node.Name}'");
			// nested property
			AddProperty(builder, parts[1], node);
		}
		else
		{
			Children.Value.Add(node);
		}
	}

	public static Object? GetNodeValue(NodeBuilder builder, XamlNode node)
	{
		if (node.IsSimpleContent)
			return node.SimpleContent;
		if (node.Name.Contains('.'))
		{
			var ch = node.Children.Value[0];
			return builder.BuildNode(ch);
		}
		return node.SimpleContent;
	}


	public static Boolean IsNestedProperty(XamlNode node, Type nodeType)
	{
		if (!node.Name.Contains('.'))
			return false;
		var split = node.Name.Split('.');
		if (split.Length != 2)
			throw new XamlException($"Invalid property name {node.Name}");
		// firts part is class or base class
		var className = split[0];
		var bt = nodeType;
		while (bt != null)
		{
			if (className == bt.Name)
				return true;
			bt = bt.BaseType;
		}
		return false;
	}

	public void AddProperty(NodeBuilder builder, String name, XamlNode node)
	{
		var td = builder.GetNodeDescriptor(Name);
		if (td == null)
			return;
		Object? propValue;
		if (IsNestedProperty(node, td.NodeType))
			propValue = td.BuildNestedProperty(builder, td.MakeName(name), node);
		else
			propValue = builder.BuildNode(node);
		if (propValue != null)
			Properties.Add(td.MakeName(name), propValue);
	}

	public void AddProperty(NodeBuilder builder, String name, String value)
	{
		var prop = builder.QualifyPropertyName(name);
		if (prop.Special)
			Properties.Add(prop.Name, new SpecialPropertyDescriptor(value));
		else if (name.Contains('.'))
			AttachedProperties.Value.Add(new XamlAttachedElem(name, value));
		else
		{
			var td = builder.GetNodeDescriptor(Name);
			if (td == null)
				return;
			if (value == null)
				return;
			if (builder.EnableMarkupExtensions)
			{
				if (value.StartsWith("{}"))
					Properties.Add(td.MakeName(prop.Name), value[2..]); // escape {}
				else if (value.StartsWith('{') && value.EndsWith('}'))
					Extensions.Add(new XamlExtensionElem(td.GetPropertyInfo(td.MakeName(prop.Name)), builder.ParseExtension(value)));
				else
					Properties.Add(td.MakeName(prop.Name), value);
			}
			else
				Properties.Add(td.MakeName(prop.Name), value);
		}
	}

	public void AddConstructorArgument(String value)
	{
		_ctorArgument = value;
	}

	public void ProcessAttachedProperties(NodeBuilder builder, Object target)
	{
		if (!AttachedProperties.IsValueCreated)
			return;
		var propManager = builder.AttachedPropertyManager;
		if (propManager != null)
		{
			foreach (var ap in AttachedProperties.Value)
				propManager.SetProperty(ap.Name, target, ap.Value);
		} 
		else
		{
			foreach (var ap in AttachedProperties.Value)
			{
				var aps = ap.Name.Split('.');
				if (aps.Length != 2)
					continue;
				var elemDescr = builder.GetNodeDescriptor(aps[0]) 
					?? throw new XamlException($"Element descriptor not found {aps[0]}");
                elemDescr.SetAttachedPropertyValue(aps[1], target, ap.Value);
			}
		}
	}
}
