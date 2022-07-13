// Copyright © 2021-2022 Alex Kukhtin. All rights reserved.

using System.Reflection;

namespace A2v10.System.Xaml;

public record XamlExtensionElem(PropertyInfo PropertyInfo, MarkupExtension Element);
public record XamlAttachedElem(String Name, Object Value);

public class XamlNode
{
	public XamlNode(String name)
	{
		Name = name;
	}
	public String Name { get; }
	public String? ConstructorArgument => _ctorArgument;

	private String? _ctorArgument;

	public Lazy<List<XamlNode>> Children = new();
	public readonly Dictionary<String, Object> Properties = new();
	public readonly List<XamlExtensionElem> Extensions = new();
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
			if (parts[0] == this.Name)
			{
				// nested property
				AddProperty(builder, parts[1], node);
			}
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


	public void AddProperty(NodeBuilder builder, String name, XamlNode node)
	{
		var td = builder.GetNodeDescriptor(Name);
		if (td == null)
			return;
		Object? propValue;
		if (node.Name == $"{Name}.{name}")
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
			if (value != null && value.StartsWith("{") && value.EndsWith("}") && builder.EnableMarkupExtensions)
				Extensions.Add(new XamlExtensionElem(td.GetPropertyInfo(td.MakeName(prop.Name)), builder.ParseExtension(value)));
			else if (value != null)
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
				var elemDescr = builder.GetNodeDescriptor(aps[0]);
				if (elemDescr == null)
					throw new XamlException($"Element descriptor not found {aps[0]}");
				elemDescr.SetAttachedPropertyValue(aps[1], target, ap.Value);
			}
		}
	}
}
