// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

using System.Collections;
using System.Reflection;

namespace A2v10.System.Xaml;
public class TypeDescriptor(Type nodeType, String typeName,
	Dictionary<String, PropertyDescriptor> props, Func<XamlNode, Object?> buildNode)
{
	public String TypeName { get; } = typeName;
	public Type NodeType { get; } = nodeType;
	public Dictionary<String, PropertyDescriptor> Properties { get; } = props;
	public Func<XamlNode, Object?> BuildNode { get; } = buildNode;

	public Func<Object>? Constructor { get; init; }
	public Func<String, Object>? ConstructorString { get; init; }
	public Func<XamlServiceProvider, Object>? ConstructorService { get; init; }

	public String? ContentProperty { get; init; }

	public Action<Object,Object>? AddCollection { get; init; }
	public Action<Object, String, Object>? AddDictionary { get; init; }

	public Dictionary<String, AttachedPropertyDescriptor>? AttachedProperties { get; init; }

	public Boolean IsCamelCase { get; init; }


	public String MakeName(String name)
	{
		if (IsCamelCase)
		{
			// source: camelCase
			// code: PascalCase
			return name.ToPascalCase();
		}
		return name;
	}

	public void SetPropertyValue(Object instance, String name, Object value)
	{
		if (!Properties.TryGetValue(name, out PropertyDescriptor? propDef))
			throw new XamlException($"Property {name} not found in type {TypeName}");
		var propInfo = propDef.PropertyInfo;
		var val = PropertyConvertor.ConvertValue(value, propInfo.PropertyType, propDef.TypeConverter);
		if (propDef.AddMethod != null && !propInfo.CanWrite)
		{
			if (value is ICollection collSource)
			{
				// copy from source value
				var target = propInfo.GetValue(instance) ?? throw new XamlException("Invalid target value");
				foreach (var elem in collSource)
					propDef.AddMethod(target, elem);
			}
		}
		else if (!propInfo.CanWrite)
			throw new XamlException($"Property {propInfo.PropertyType} is read only");
		else
			propInfo.SetValue(instance, val);
	}

	public void SetTextContent(Object instance, String? content)
	{
		if (ContentProperty == null)
			throw new XamlException($"ContentProperty not found in type {TypeName}");
		if (String.IsNullOrEmpty(content))
			return;
		if (!Properties.TryGetValue(ContentProperty, out PropertyDescriptor? contDef))
		{
			// May be readonly collection?
			throw new XamlException($"Property {ContentProperty} not found in type {TypeName}");
		}
		var contProp = contDef.PropertyInfo;
		// TODO: GetTypeDescriptor for contProp.PropertyType. May be collection?
		var val = PropertyConvertor.ConvertValue(content, contProp.PropertyType, contDef.TypeConverter);
		contProp.SetValue(instance, val);
	}

	public void AddChildren(Object instance, Object elem, XamlNode node)
	{
		if (ContentProperty == null)
		{
			if (AddCollection != null)
				AddCollection(instance, elem);
			else if (AddDictionary != null)
                AddDictionary(instance, node.XKeyName, elem);
			return;
		}
		if (!Properties.TryGetValue(ContentProperty, out PropertyDescriptor? contDef))
			return;
		var contProp = contDef.PropertyInfo;
		if (contProp.PropertyType.IsPrimitive || contProp.PropertyType == typeof(Object))
		{
			contProp.SetValue(instance, elem);
			return;
		}
		var contObj = contProp.GetValue(instance);
		if (contObj == null)
		{
			if (contProp.CanWrite && contProp.PropertyType.IsAssignableFrom(elem.GetType()))
				contProp.SetValue(instance, elem);
			else
			{
				if (contDef.Constructor == null)
					throw new XamlException("Constructor not defined");
				contObj = contDef.Constructor();
				contProp.SetValue(instance, contObj);
			}
		}
		if (contObj != null)
		{
            if (AddCollection != null)
                AddCollection(contObj, elem);
            else if (AddDictionary != null)
                AddDictionary(contObj, node.XKeyName, elem);
        }
    }

	public Object? BuildNestedProperty(NodeBuilder builder, String name, XamlNode node)
	{
		if (!Properties.TryGetValue(name, out PropertyDescriptor? propDef))
			throw new XamlException($"Property {name} not found");
		if (node == null)
			return null;
		return propDef.BuildElement(builder, node);
	}


	public PropertyInfo GetPropertyInfo(String name)
	{
		if (Properties.TryGetValue(name, out PropertyDescriptor? propDef))
			return propDef.PropertyInfo;
		throw new XamlException($"Property {name} not found");
	}

	public void SetAttachedPropertyValue(String prop, Object target, Object? value)
	{
		if (AttachedProperties == null || value == null)
			return;
		if (AttachedProperties.TryGetValue(prop, out AttachedPropertyDescriptor? descr))
		{
			if (descr.PropertyType.IsEnum)
				value = Enum.Parse(descr.PropertyType, value.ToString()!);
			else if (descr.TypeConverter != null)
			{
				value = descr.TypeConverter.ConvertFromString(value.ToString()!);
			}
			else
				value = Convert.ChangeType(value, descr.PropertyType);
			if (value == null)
				return;
			descr.Lambda(target, value);
		}
	}

}

