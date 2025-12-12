// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Reflection;
using System.Collections;

namespace A2v10.System.Xaml;

public record XamlWriteProp(String Name, Object? Value, Boolean IsContent)
{
    public Boolean IsEmpty => Value == null;
}

public class XamlWriteNode(String name)
{
    public String Name { get; init; } = name;
    public String? Namespace { get; init; }
    public String? ContentProperty { get; init; }
    public List<XamlWriteProp> Properties { get; } = [];
    public static XamlWriteNode Create(Object obj)
    {
        var tp = obj.GetType();
        var cp = tp.GetCustomAttribute<ContentPropertyAttribute>()?.Name;
        String nsp = $"clr-namespace:{tp.Namespace};assembly={tp.Assembly.GetName().Name}";
        var node = new XamlWriteNode(tp.Name)
        {
            Namespace = nsp,
            ContentProperty = cp,
        };
        node.ParseProperties(obj);
        return node;
    }

    public void ParseProperties(Object obj)
    {
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            ParseOneProperty(prop, prop.GetValue(obj));
    }
    public void ParseOneProperty(PropertyInfo prop, Object? value)
    {
        if (value == null)
            return;
        Boolean isContentProp = prop.Name == ContentProperty;
        if (value is String strVal)
        {
            if (String.IsNullOrEmpty(strVal))
                return;
             Properties.Add(new XamlWriteProp(prop.Name, strVal, isContentProp));
        }
        else if (value is Boolean boolVal)
            Properties.Add(new XamlWriteProp(prop.Name, boolVal.ToString(), isContentProp));
        else if (prop.PropertyType.IsEnum)
        {
            var enumText = value.ToString();
            var defEnum = Enum.GetName(prop.PropertyType, 0);
            if (enumText != defEnum)
                Properties.Add(new XamlWriteProp(prop.Name, enumText, isContentProp));
        }
        else if (prop.PropertyType == typeof(Object))
        {
            if (value is String strVal2)
                Properties.Add(new XamlWriteProp(prop.Name, strVal2, isContentProp));
            else
                Properties.Add(new XamlWriteProp(prop.Name, XamlWriteNode.Create(value), isContentProp));
        }
        var addMethod = prop.PropertyType.GetMethod("Add");
        if (addMethod != null)
        {
            var chProp = new XamlWriteProp(prop.Name, null, isContentProp);
            Properties.Add(chProp);
            // collection
            if (value is IEnumerable iEnum)
            {
                var list = new List<Object>();
                foreach (var item in iEnum)
                {
                    if (item is String)
                        list.Add(item);
                    else
                        list.Add(XamlWriteNode.Create(item));
                }
                Properties.Add(new XamlWriteProp(prop.Name, list, isContentProp));
            }
        }
    }
}
