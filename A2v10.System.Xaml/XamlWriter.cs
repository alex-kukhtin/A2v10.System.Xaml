// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Linq;
using System.Xml;

namespace A2v10.System.Xaml;

public class XamlWriter
{
    private Boolean _hasDictionary;
    private String? _rootNamespace;

    private const String X_NS = "http://schemas.microsoft.com/winfx/2006/xaml";
    public String GetXaml(Object obj)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true,
            IndentChars = "\t"
        };

        XamlWriteNode writeNode = XamlWriteNode.Create(obj, null);

        _hasDictionary = writeNode.AllElements().Any(x => x.IsDictionary);
        _rootNamespace = writeNode.Namespace;

        using var sw = new StringWriter();
        using (var writer = XmlWriter.Create(sw, settings))
        {
            writer.WriteStartDocument();    
            WriteNode(writer, writeNode, true);
            writer.WriteEndDocument();
        }
        return sw.ToString();
    }

    void WriteNode(XmlWriter writer, XamlWriteNode node, Boolean bRoot)
    {
        writer.WriteStartElement(node.Name, node.Namespace);

        // first attribute!!!!
        if (bRoot)
            writer.WriteAttributeString("xmlns", String.Empty, null, node.Namespace);

        if (bRoot && _hasDictionary)
            writer.WriteAttributeString("xmlns", "x", null, X_NS);

        // first path - string attributes 
        foreach (var prop in node.Properties.Where(p => !p.IsEmpty && p.Value is String && !p.IsContent))
        {
            if (prop.Name.StartsWith("x:"))
                writer.WriteAttributeString(prop.Name[2..], X_NS, prop.Value!.ToString());
            else
                writer.WriteAttributeString(prop.Name, prop.Value!.ToString());
        }
        // second path - string content
        foreach (var prop in node.Properties.Where(p => !p.IsEmpty && p.Value is String && p.IsContent))
        {
            writer.WriteString(prop.Value!.ToString());
        }
        // third path - node content
        foreach (var prop in node.Properties.Where(p => !p.IsEmpty && p.Value is not String))
        {
            if (!prop.IsContent)
                writer.WriteStartElement($"{node.Name}.{prop.Name}");
            if (prop.Value is XamlWriteNode propNode)
            {
                CheckNamespace(node, prop, propNode);
                WriteNode(writer, propNode, false);
            }
            else if (prop.Value is List<Object> listValues)
            {
                foreach (var item in listValues)
                {
                    if (item is String strValue)
                        writer.WriteString(strValue);
                    else if (item is XamlWriteNode writeNode)
                    {
                        CheckNamespace(node, prop, writeNode);
                        WriteNode(writer, writeNode, false);
                    }
                }
            }
            if (!prop.IsContent)
                writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    /* An element from another CLR namespace makes XmlWriter redefine the default xmlns on it, and
     * the reader does not accept a redefinition (NodeBuilder.AddNamespace keeps the first): the
     * class is looked up in the root namespace and is not found - or a namesake is. A wrapper node
     * has no namespace of its own and inherits the document's.
     */
    void CheckNamespace(XamlWriteNode owner, XamlWriteProp prop, XamlWriteNode child)
    {
        if (child.Namespace != null && child.Namespace != _rootNamespace)
            throw new XamlException($"Cannot write {child.Name} in property '{owner.Name}.{prop.Name}': namespace '{child.Namespace}' is not the document namespace '{_rootNamespace}'");
    }
}
