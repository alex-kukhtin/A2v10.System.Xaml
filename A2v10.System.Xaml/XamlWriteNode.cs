// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Reflection;
using System.Collections;
using System.Linq;

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
    public Boolean IsDictionary { get; private set; }
    public List<XamlWriteProp> Properties { get; } = [];
    public static XamlWriteNode Create(Object obj, Object? parent)
    {
        var tp = obj.GetType();
        String? cp = null;
        if (tp.GetCustomAttribute<ContentAsXamlAttrAttribute>() == null)
            cp = tp.GetCustomAttribute<ContentPropertyAttribute>()?.Name;

        var ignoreProps = tp.GetCustomAttribute<IgnoreWritePropertiesAttribute>();
        String nsp = $"clr-namespace:{tp.Namespace};assembly={tp.Assembly.GetName().Name}";
        var node = new XamlWriteNode(tp.Name)
        {
            Namespace = nsp,
            ContentProperty = cp,
        };
        if (parent == null)
        {
            if (node.AddCollectionNode(tp, obj, parent))
                return node;
        }
        node.ParseProperties(obj, ignoreProps);
        if (parent != null)
            node.ParseAttached(obj, parent);
        return node;
    }

    private readonly static HashSet<String> _skippedProps =
    [..
        "AttachedPropertyManager,BindImpl,Bindings,Attach,IsParentToolBar".Split(',')
    ];
    public void ParseProperties(Object obj, IgnoreWritePropertiesAttribute? ignoreProps)
    {
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !_skippedProps.Contains((p.Name)))
            )
        {
            if (ignoreProps != null && ignoreProps.Contains(prop.Name))
                continue;
            if (prop.GetIndexParameters().Length == 0)
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
        // underlaying
        var enumUnder = Nullable.GetUnderlyingType(prop.PropertyType);
        if (enumUnder != null && enumUnder.IsEnum)
        {
            var enumText = value.ToString();
            var defEnum = Enum.GetName(enumUnder, 0);
            if (enumText != defEnum)
                Properties.Add(new XamlWriteProp(prop.Name, enumText, isContentProp));
            return;
        }

        (Boolean has, Object? val) = value switch
        {
            String isStr => (true, !String.IsNullOrWhiteSpace(isStr) ? isStr : null),
            Boolean bVal => (true, bVal ? bVal.ToString() : null),
            Int32 intVal => (true, intVal != 0 ? intVal.ToString() : null),
            UInt32 uintVal => (true, uintVal != 0 ? uintVal.ToString() : null),
            _ => (false, null)
        };
        if (has)
        {
            if (val != null)
                Properties.Add(new XamlWriteProp(prop.Name, val, isContentProp));
            return;
        }


        if (value is IXamlConverter xamlConverter)
            Properties.Add(new XamlWriteProp(prop.Name, xamlConverter.ToXamlString(), isContentProp));
        else
        {
            if (AddCollectionProp(prop, value, parent))
                return;
            Properties.Add(new XamlWriteProp(prop.Name, XamlWriteNode.Create(value, parent), isContentProp));
        }
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
        Boolean isDict = addMethod.GetParameters().Length == 2;
        Boolean hasWrapper = prop.PropertyType.GetCustomAttribute<WrapContentAttribute>() != null;
        // collection
        if (value is IEnumerable iEnum)
        {
            var list = new List<Object>();
            foreach (var item in iEnum)
            {
                if (item is String)
                    list.Add(item);
                else if (isDict && item is KeyValuePair<String, Object> keyValuePair)
                {
                    var obj = XamlWriteNode.Create(keyValuePair.Value, parent);
                    obj.Properties.Add(new XamlWriteProp("x:Key", keyValuePair.Key, false));
                    list.Add(obj);
                }
                else
                    list.Add(XamlWriteNode.Create(item, parent));
            }
            if (list.Count > 0)
            {
                if (!hasWrapper)
                {
                    Properties.Add(new XamlWriteProp(prop.Name, list, isContentProp));
                }
                else
                {
                    // wrapper
                    var wrapProp = new XamlWriteProp(prop.PropertyType.Name, list, true);
                    var wrapObj = new XamlWriteNode(prop.PropertyType.Name)
                    {
                        IsDictionary = isDict
                    };
                    wrapObj.Properties.Add(wrapProp);
                    Properties.Add(new XamlWriteProp(prop.Name, wrapObj, false));
                }
            }
            return true;
        }
        return false;
    }

    KeyValuePair<String, Object>? GenericKeyValuePair(Object item)
    {
        if (item is KeyValuePair<String, Object> kvp)
            return kvp;
        var tp = item.GetType();
        if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var keyProp = tp.GetProperty("Key")!;
            var valProp = tp.GetProperty("Value")!;
            return new KeyValuePair<String, Object>(keyProp.GetValue(item)!.ToString()!, valProp.GetValue(item)!);
        }
        return null;
    }

    Boolean AddCollectionNode(Type type, Object? value, Object? parent)
    {
        var addMethod = type.GetMethod("Add");
        if (addMethod == null)
            return false;
        Boolean isDict = addMethod.GetParameters().Length == 2;
        IsDictionary = isDict;
        // collection
        if (value is IEnumerable iEnum)
        {
            var list = new List<Object>();
            foreach (var item in iEnum)
            {
                if (item is String)
                    list.Add(item);
                else if (isDict)
                {
                    var kvp = GenericKeyValuePair(item);
                    if (kvp != null)
                    {
                        var obj = XamlWriteNode.Create(kvp.Value.Value, parent);
                        obj.Properties.Add(new XamlWriteProp("x:Key", kvp.Value.Key, false));
                        list.Add(obj);
                    }
                }
                else
                    list.Add(XamlWriteNode.Create(item, parent));
            }
            if (list.Count > 0)
            {
                Properties.Add(new XamlWriteProp(type.Name, list, true));
            }
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

    public IEnumerable<XamlWriteNode> AllElements()
    {
        yield return this;
        foreach (var prop in Properties)
        {
            if (prop.Value is XamlWriteNode obj1)
                foreach (var n1 in obj1.AllElements())
                    yield return n1;
            else if (prop.Value is IEnumerable iEnum)
            {
                foreach (var item in iEnum)
                {
                    if (item is XamlWriteNode obj2)
                        foreach (var n2 in obj2.AllElements())
                            yield return n2;
                }
            }
        }
    }
}
