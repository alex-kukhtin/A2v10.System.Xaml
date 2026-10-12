// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace A2v10.System.Xaml;

public record XamlWriteProp(String Name, Object? Value, Boolean IsContent, Boolean IsCData = false)
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
        // a declaration error: thrown whatever the value, null included
        Boolean isCData = prop.GetCustomAttribute<WriteAsCDataAttribute>() != null;
        if (isCData && prop.PropertyType != typeof(String))
            throw new XamlException($"Cannot write property '{Name}.{prop.Name}': [WriteAsCData] requires a String property, not {prop.PropertyType.Name}");

        if (ParseBinding(prop, parent))
            return;

        if (value == null)
            return;

        if (!prop.CanWrite)
            return;

        Boolean isContentProp = prop.Name == ContentProperty;

        // for a nullable property null already means "not set",
        // so an explicit false/0/enum(0) is a value and is always written
        var typeUnder = Nullable.GetUnderlyingType(prop.PropertyType);
        Boolean isNullable = typeUnder != null;

        if (prop.PropertyType.IsEnum)
        {
            var enumText = value.ToString();
            var defEnum = Enum.GetName(prop.PropertyType, 0);
            if (enumText != defEnum)
                Properties.Add(new XamlWriteProp(prop.Name, enumText, isContentProp));
            return;
        }
        // underlaying
        if (typeUnder != null && typeUnder.IsEnum)
        {
            Properties.Add(new XamlWriteProp(prop.Name, value.ToString(), isContentProp));
            return;
        }

        if (value is String str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return;
            if (!isCData)
                Properties.Add(new XamlWriteProp(prop.Name, Literal(str, isContentProp), isContentProp));
            // XmlWriter splits ']]>' into two CDATA sections, and the reader cannot read two
            // text nodes in a row. Element text is not parsed by the reader, so no Literal()
            else if (str.Contains("]]>"))
                throw new XamlException($"Cannot write property '{Name}.{prop.Name}' as CDATA: the value contains ']]>'. Insert a space: ']] >'");
            else
                Properties.Add(new XamlWriteProp(prop.Name, str, isContentProp, IsCData: true));
            return;
        }

        var valueType = value.GetType();
        // a non-nullable property has no "not set" but its default: 0, false, DateTime.MinValue are not written
        Boolean isDefault = valueType.IsValueType && !isNullable && value.Equals(Activator.CreateInstance(valueType));

        if (value is IConvertible scalar && Type.GetTypeCode(valueType) is not (TypeCode.Object or TypeCode.DBNull))
        {
            if (!isDefault)
                Properties.Add(new XamlWriteProp(prop.Name, Literal(ScalarText(scalar), isContentProp), isContentProp));
            return;
        }

        if (value is IXamlConverter xamlConverter)
            Properties.Add(new XamlWriteProp(prop.Name, Literal(xamlConverter.ToXamlString(), isContentProp), isContentProp));
        else
        {
            if (AddCollectionProp(prop, value, parent))
                return;
            // a struct written as an element is never read back: the reader finds no parameterless
            // constructor on it (and a Guid or a TimeSpan would be written empty besides)
            if (valueType.IsValueType)
            {
                if (isDefault)
                    return;
                throw new XamlException($"Cannot write property '{Name}.{prop.Name}': {valueType.Name} has no text form (no IXamlConverter)");
            }
            Properties.Add(new XamlWriteProp(prop.Name, XamlWriteNode.Create(value, parent), isContentProp));
        }
    }

    /* The text the reader parses back to the same value (PropertyConvertor.ParseText): the invariant
     * culture, and for a DateTime the round-trip format - the default one drops the fraction of a
     * second and the Kind. A Double prints its shortest exact form.
     */
    static String ScalarText(IConvertible scalar)
    {
        if (scalar is DateTime dt)
            return dt.ToString("o", CultureInfo.InvariantCulture);
        return scalar.ToString(CultureInfo.InvariantCulture);
    }

    /* The reader takes an attribute that starts with '{' for a markup extension and strips a
     * leading '{}' (XamlNode.AddProperty), so a literal attribute that starts with '{' is written
     * behind '{}': "{x: 1}" came back as an extension, "{}abc" as "abc". Only a literal attribute -
     * markup from IBindWriter is markup, and the reader parses neither element text (isContent),
     * nor an attached value, nor x:Key: '{}' written there would stay in the value.
     */
    static String? Literal(String? text, Boolean isContent)
    {
        if (!isContent && text != null && text.StartsWith('{'))
            return "{}" + text;
        return text;
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
                // any KeyValuePair<,>: Dictionary<String, UIElementBase> is not KeyValuePair<String, Object>,
                // and its items were written as elements named KeyValuePair`2
                else if (isDict && GenericKeyValuePair(item) is { } keyValuePair)
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
            // the reader converts x:Key back in the invariant culture: ToString() wrote a Double 1.5 as "1,5"
            var key = keyProp.GetValue(item)!;
            var keyText = key is IConvertible scalar ? ScalarText(scalar) : key.ToString()!;
            return new KeyValuePair<String, Object>(keyText, valProp.GetValue(item)!);
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
