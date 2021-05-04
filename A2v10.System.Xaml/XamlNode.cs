﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace A2v10.System.Xaml
{
	public record XamlExtensionElem(PropertyInfo PropertyInfo, MarkupExtension Element);
	public record XamlAttachedElem(String Name, Object Value);

	public class XamlNode
	{
		public String Name { get; init; }
		public String TextContent { get; set; }
		public String ConstructorArgument => _ctorArgument;

		private String _ctorArgument;

		public Lazy<List<XamlNode>> Children = new();
		public readonly Dictionary<String, Object> Properties = new();
		public readonly List<XamlExtensionElem> Extensions = new();
		public readonly Lazy<List<XamlAttachedElem>> AttachedProperties = new();

		public Boolean HasChildren => Children.IsValueCreated && Children.Value.Count > 0;
		public Boolean HasExtensions => Extensions!= null && Extensions.Count > 0;


		public void SetContent(String text)
		{
			TextContent = text;
		}

		public void AddChildren(XamlNode node, NodeBuilder builder)
		{
			if (node.Name.Contains("."))
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

		public static Object GetNodeValue(NodeBuilder builder, XamlNode node)
		{
			if (!node.HasChildren)
				return node.TextContent;
			if (node.Name.Contains('.'))
			{
				var ch = node.Children.Value[0];
				return builder.BuildNode(ch);
			}
			return node.TextContent;
		}


		public void AddProperty(NodeBuilder builder, String name, XamlNode node)
		{
			var td = builder.GetNodeDescriptor(Name);
			Object propValue;
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
				if (value != null && value.StartsWith("{") && value.EndsWith("}") && builder.EnableMarkupExtensions)
					Extensions.Add(new XamlExtensionElem(td.GetPropertyInfo(td.MakeName(prop.Name)), builder.ParseExtension(value)));
				else if (td != null)
					Properties.Add(td.MakeName(prop.Name), value); // nd.BuildProperty(propName, value));
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
					elemDescr.SetAttachedPropertyValue(aps[1], target, ap.Value);
				}
			}
		}
	}
}
