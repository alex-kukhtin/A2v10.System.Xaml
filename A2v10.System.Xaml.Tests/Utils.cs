
using System.Linq;
using System.Xml.Linq;

namespace A2v10.System.Xaml.Tests;

internal static class Utils
{

    public static XText NormalizeText(XText tx)
    {
        return new XText(tx.Value.Trim());
    }

    public static XElement Normalize(XElement el)
    {
        return new XElement(el.Name,
            el.Attributes().OrderBy(a => a.Name.ToString()),
            el.Nodes().Select(n => n is XElement e ? Normalize(e) : n is XText t ? NormalizeText(t) : n)
        );
    }


    public static bool XmlEquals(string xml1, string xml2)
    {
        var doc1 = Normalize(XDocument.Parse(xml1, LoadOptions.None).Root!);
        var doc2 = Normalize(XDocument.Parse(xml2, LoadOptions.None).Root!);
        return XNode.DeepEquals(doc1, doc2);
    }

}
