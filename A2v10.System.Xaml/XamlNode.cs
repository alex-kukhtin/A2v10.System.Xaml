// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.Reflection;

namespace A2v10.System.Xaml;

public record XamlExtensionElem(PropertyInfo PropertyInfo, MarkupExtension Element);
public record XamlAttachedElem(String Name, Object Value);

public class XamlNode(String name)
{
	public String Name { get; } = name;
	public String? ConstructorArgument => _ctorArgument;
	// where the markup put the node; 0 - the reader reports no position
	public Int32 LineNumber { get; init; }
	public Int32 LinePosition { get; init; }

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
			if (!Properties.TryGetValue("Key", out var keyProp) || keyProp is not SpecialPropertyDescriptor spd) 
				throw new XamlException($"Property Key not found in type {Name}");
			return spd.Name;
		}
	}

	public void SetContent(String text, Int32 lineNumber = 0, Int32 linePosition = 0)
	{
		Children.Value.Add(new XamlTextContent() { Text = text, LineNumber = lineNumber, LinePosition = linePosition });
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


	/* A dot in a name is three different things, and this is what tells them apart.
	 *   'Type.Property'  where Type is this node's type OR any of its base types -> a nested
	 *                    property of this very element (<Grid.Rows>), so the value is built into
	 *                    that property. Hence the walk up BaseType: the markup may name the base
	 *                    class that declares the property.
	 *   'Other.Property' -> an attached property, kept aside as XamlAttachedElem and applied to
	 *                    the target later by ProcessAttachedProperties.
	 *   anything with more than one dot -> an error, and it is raised here rather than guessed.
	 * Asked on the ELEMENT road only (AddChildren -> AddProperty(.., XamlNode)). A dotted
	 * ATTRIBUTE needs no test: markup has no way to spell a nested property as an attribute, so
	 * 'Grid.Row="1"' is attached by definition - which is why the other AddProperty sends every
	 * dotted name straight to AttachedProperties.
	 */
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
			if (className == bt.Name || className.Contains(':') && className.EndsWith(bt.Name))
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
			SetOnce(td.MakeName(name), propValue);
	}

	/* A property is set once. Properties and Extensions are one namespace of names: an attribute
	 * extension goes to Extensions, so Content="{Bind X}" with <Button.Content> never met in a
	 * dictionary - both were applied and the deferred binding silently won. Attributes are read
	 * before child elements, so the second setter is always the one that throws.
	 */
	void CheckOnce(String key)
	{
		if (Properties.ContainsKey(key) || Extensions.Exists(x => x.PropertyInfo.Name == key))
			throw new XamlException($"Property '{key}' is set more than once in '{Name}'");
	}

	void SetOnce(String key, Object value)
	{
		CheckOnce(key);
		Properties.Add(key, value);
	}

	public void AddProperty(NodeBuilder builder, String name, String value)
	{
		AddProperty(builder, name, value, isLiteral: false);
	}

	/* Which value is a markup extension, and who decides.
	 *   An attribute value: anything that starts with '{'. The parser owns the rest of it - a tail
	 *   after the closing brace or a missing brace is its error, not a literal; a test on the last
	 *   character here let "{Bind Name} - {Bind Code}" and "{Bind Name" pass silently.
	 *   A value inside an extension: the parser already knows from the token, so isLiteral. A
	 *   quoted value ('{0} шт', '{0:n2}') is a literal whatever it starts with; only a braced
	 *   token is a nested extension.
	 * A leading '{}' is stripped on both roads: it means "a literal follows" everywhere, and
	 * '{}{0:n2}' was how a quoted format was written before the quotes alone were enough.
	 */
	internal void AddProperty(NodeBuilder builder, String name, String value, Boolean isLiteral)
	{
		var prop = builder.QualifyPropertyName(name);
		if (prop.Special)
			SetOnce(prop.Name, new SpecialPropertyDescriptor(value));
		else if (name.Contains('.'))
		{
			if (builder.CheckAttachedProperty(name))
				AttachedProperties.Value.Add(new XamlAttachedElem(name, value));
		}
		else
		{
			var td = builder.GetNodeDescriptor(Name);
			if (td == null)
				return;
			if (value == null)
				return;
			var key = td.MakeName(prop.Name);
			if (builder.EnableMarkupExtensions)
			{
				if (value.StartsWith("{}"))
					SetOnce(key, value[2..]); // escape {}
				else if (!isLiteral && value.StartsWith('{'))
				{
					CheckOnce(key);
					Extensions.Add(new XamlExtensionElem(td.GetPropertyInfo(key), builder.ParseExtension(value)));
				}
				else
					SetOnce(key, value);
			}
			else
				SetOnce(key, value);
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
			// two parts and a known owner - NodeBuilder.CheckAttachedProperty saw to it
			foreach (var ap in AttachedProperties.Value)
			{
				var aps = ap.Name.Split('.');
				var elemDescr = builder.GetNodeDescriptor(aps[0])
					?? throw new XamlException($"Element descriptor not found {aps[0]}");
				elemDescr.SetAttachedPropertyValue(elemDescr.MakeName(aps[1]), target, ap.Value);
			}
		}
	}
}
