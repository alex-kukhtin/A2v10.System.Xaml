// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.Collections;
using System.Reflection;

namespace A2v10.System.Xaml;

public class TypeDescriptor(Type nodeType, String typeName,
    Dictionary<String, PropertyDescriptor> props)
{
    public String TypeName { get; } = typeName;
    public Type NodeType { get; } = nodeType;
    public Dictionary<String, PropertyDescriptor> Properties { get; } = props;

    public Func<Object>? Constructor { get; init; }
    public Func<String, Object>? ConstructorString { get; init; }
    public Func<XamlServiceProvider, Object>? ConstructorService { get; init; }

    public String? ContentProperty { get; init; }

    public Action<Object, Object>? AddCollection { get; init; }
    public Action<Object, String, Object>? AddDictionary { get; init; }

    public Dictionary<String, AttachedPropertyDescriptor>? AttachedProperties { get; init; }
    // [AttachedProperties] of the type, in both modes; AttachedProperties holds only those with a static setter
    internal String[] AttachedNames { get; init; } = [];

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

    public void SetPropertyValue(Object instance, String name, Object value, Boolean skipUnknown)
    {
        if (!Properties.TryGetValue(name, out PropertyDescriptor? propDef))
        {
            if (skipUnknown)
                return;
            throw new XamlException($"Property {name} not found in type {TypeName}");
        }
        var propInfo = propDef.PropertyInfo;
        var val = PropertyConvertor.ConvertValue(value, propInfo.PropertyType, propDef.TypeConverter, $"property '{name}'");
        if (propDef.AddMethod != null && !propInfo.CanWrite)
        {
            if (val is ICollection collSource)
            {
                // copy from source value
                var target = propInfo.GetValue(instance) ?? throw new XamlException("Invalid target value");
                foreach (var elem in collSource)
                    propDef.AddMethod(target, elem);
            }
        }
        else if (!propInfo.CanWrite)
            throw new XamlException($"Property {propInfo.Name} is read only");
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
        var val = PropertyConvertor.ConvertValue(content, contProp.PropertyType, contDef.TypeConverter, $"property '{ContentProperty}'");
        contProp.SetValue(instance, val);
    }

    public void AddChildren(Object instance, Object elem, XamlNode node)
    {
        if (ContentProperty == null)
        {
            if (AddCollection != null)
                AddCollection(instance, elem);
            else
                AddDictionary?.Invoke(instance, node.XKeyName, elem);
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
            else
                AddDictionary?.Invoke(contObj, node.XKeyName, elem);
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
        if (value == null)
            return;
        // the name passed NodeBuilder.CheckAttachedProperty, so only the setter can be missing:
        // a manager-mode owner read in the internal mode
        if (AttachedProperties == null || !AttachedProperties.TryGetValue(prop, out AttachedPropertyDescriptor? descr))
            throw new XamlException($"Attached property '{NodeType.Name}.{prop}' has no static setter Set{prop} in type {TypeName}");
        // the same conversion as an ordinary property: an enum ignores case, the culture is invariant
        value = PropertyConvertor.ConvertValue(value, descr.PropertyType, descr.TypeConverter, $"property '{NodeType.Name}.{prop}'");
        if (value == null)
            return;
        descr.Lambda(target, value);
    }

}

