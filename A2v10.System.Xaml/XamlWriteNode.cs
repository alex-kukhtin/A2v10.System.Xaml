// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Reflection;
using System.Collections;
using System.Linq;
using System.ComponentModel;

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
    public static XamlWriteNode Create(Object obj, Object? parent)
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
        if (parent != null)
            node.ParseAttached(obj, parent);
        return node;
    }

    private readonly static HashSet<String> _skippedProps =
    [.. 
        "AttachedPropertyManager,BindImpl".Split(',')
    ];
    public void ParseProperties(Object obj)
    {
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !_skippedProps.Contains((p.Name)))
            )
        {
            ParseOneProperty(prop, prop.GetValue(obj), obj);
        }
    }
    public void ParseOneProperty(PropertyInfo prop, Object? value, Object parent)
    {
        if (ParseBinding(prop, parent))
            return;

        if (value == null)
            return;

        Boolean isContentProp = prop.Name == ContentProperty;
        if (prop.PropertyType.IsEnum)
        {
            var enumText = value.ToString();
            var defEnum = Enum.GetName(prop.PropertyType, 0);
            if (enumText != defEnum)
                Properties.Add(new XamlWriteProp(prop.Name, enumText, isContentProp));
            return;
        }

        (Boolean has, Object? val) = value switch
        {
            String isStr => (true, !String.IsNullOrWhiteSpace(isStr) ? isStr : null),
            Boolean bVal => (true, bVal ? bVal.ToString() : null),
            _ => (false, null)
        };
        if (has)
        {
            if (val != null)
                Properties.Add(new XamlWriteProp(prop.Name, val, isContentProp));
            return;
        }

        if (AddCollectionProp(prop, value, parent))
            return;

        var tc = value.GetType().GetCustomAttribute<TypeConverterAttribute>();
        if (tc != null)
            Properties.Add(new XamlWriteProp(prop.Name, value.ToString(), isContentProp));
        else
            Properties.Add(new XamlWriteProp(prop.Name, XamlWriteNode.Create(value, parent), isContentProp));
    }

    Boolean ParseBinding(PropertyInfo prop, Object? value)
    {
        if (value is not IBindWriter bindWriter)
            return false;
        var propValue = bindWriter.CreateMarkup(prop.Name);
        if (propValue == null)
            return false;
        Properties.Add(new XamlWriteProp(prop.Name, propValue, false));
        return true;
    }

    Boolean AddCollectionProp(PropertyInfo prop, Object? value, Object parent)
    {
        var addMethod = prop.PropertyType.GetMethod("Add");
        if (addMethod == null)
            return false;
        Boolean isContentProp = prop.Name == ContentProperty;
        // collection
        if (value is IEnumerable iEnum)
        {
            var list = new List<Object>();
            foreach (var item in iEnum)
            {
                if (item is String)
                    list.Add(item);
                else
                    list.Add(XamlWriteNode.Create(item, parent));
            }
            if (list.Count > 0)
                Properties.Add(new XamlWriteProp(prop.Name, list, isContentProp));
            return true;
        }
        return false;
    }
    void ParseAttached(Object obj, Object? parent)
    {
        if (parent == null)
            return;
        if (parent is not ISupportAttached supAtt)
            return;
        var attPropManager = supAtt.AttachedPropertyManager;
        var tp = parent.GetType();
        var att = tp.GetCustomAttribute<AttachedPropertiesAttribute>()?.List;
        if (String.IsNullOrWhiteSpace(att))
            return;
        foreach (var attProp in att.Split(','))
        {
            var propName = $"{tp.Name}.{attProp}";
            var attVal = attPropManager.GetProperty<Object>(propName, obj);
            if (attVal != null)
                Properties.Add(new XamlWriteProp(propName, attVal.ToString(), false));
        }
    }
}
