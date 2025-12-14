// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Linq;
using System.Xml;

namespace A2v10.System.Xaml;

public class XamlWriter
{
    private Boolean _hasDictionary;

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
            Boolean isContentProp = prop.Name == node.ContentProperty;
            if (prop.Value is XamlWriteNode propNode)
            {
                if (isContentProp)
                    WriteNode(writer, propNode, false);
                else
                {
                    writer.WriteStartElement($"{node.Name}.{prop.Name}");
                    WriteNode(writer, propNode, false);
                    writer.WriteEndElement();
                }

            }
            else if (prop.Value is List<Object> listValues)
            {
                foreach (var item in listValues)
                {
                    if (item is String strValue)
                        writer.WriteString(strValue);
                    else if (item is XamlWriteNode writeNode)
                        WriteNode(writer, writeNode, false);
                }
            }
        }
        writer.WriteEndElement();
    }
}
