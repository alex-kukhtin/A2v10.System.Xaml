// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Linq;
using System.Xml;

namespace A2v10.System.Xaml;

public class XamlWriter
{
    public String GetXaml(Object obj)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true,
            IndentChars = "\t"
        };

        XamlWriteNode writeNode = XamlWriteNode.Create(obj, null);


        using var sw = new StringWriter();
        using (var writer = XmlWriter.Create(sw, settings))
        {
            writer.WriteStartDocument();    
            WriteNode(writer, writeNode);
            writer.WriteEndDocument();
        }
        return sw.ToString();
    }

    static void WriteNode(XmlWriter writer, XamlWriteNode node)
    {
        writer.WriteStartElement(node.Name, node.Namespace);

        // first path - string attributes 
        foreach (var prop in node.Properties.Where(p => !p.IsEmpty && p.Value is String && !p.IsContent))
        {
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
                    WriteNode(writer, propNode);
                else
                {
                    writer.WriteStartElement($"{node.Name}.{prop.Name}");
                    WriteNode(writer, propNode);
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
                        WriteNode(writer, writeNode);
                }
            }
        }
        writer.WriteEndElement();
    }
}
