// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml.Tests.Mock;

namespace A2v10.System.Xaml.Tests;

// [WriteAsCData]: the text comes back as is - line breaks, tabs, leading and trailing spaces
[TestCategory("Xaml.Writer")]
public partial class TextXamlWriter
{
    const String CDataCode = "\n\tconst expr = (sum) => {\n\t\treturn sum + 5;\n\t};\n  ";

    [TestMethod]
    public void CDataPropertyElementRoundTrip()
    {
        var script = new CDataScript() { Title = "T", Code = CDataCode };
        var written = XamlServices.Write(script);
        Assert.Contains("<CDataScript.Code><![CDATA[", written);
        Assert.Contains("""Title="T">""", written);

        var back = XamlServices.Parse<CDataScript>(written);
        Assert.AreEqual(CDataCode, back.Code);
        Assert.AreEqual("T", back.Title);
    }

    [TestMethod]
    public void CDataContentRoundTrip()
    {
        var script = new CDataContentScript() { Code = CDataCode };
        var written = XamlServices.Write(script);
        Assert.Contains("><![CDATA[", written);

        var back = XamlServices.Parse<CDataContentScript>(written);
        Assert.AreEqual(CDataCode, back.Code);
    }

    [TestMethod]
    public void CDataBraceNotEscaped()
    {
        // element text is never parsed by the reader, so '{}' would stay in the value
        var script = new CDataScript() { Code = "{x: 1}" };
        var back = XamlServices.Parse<CDataScript>(XamlServices.Write(script));
        Assert.AreEqual("{x: 1}", back.Code);
    }

    [TestMethod]
    public void CDataEndMarkerThrows()
    {
        var script = new CDataScript() { Code = "if (a[b[i]]>0) return;" };
        var ex = Assert.Throws<XamlException>(() => XamlServices.Write(script));
        Assert.Contains("'CDataScript.Code'", ex.Message);
        Assert.Contains("']]>'", ex.Message);
    }

    [TestMethod]
    public void CDataNonStringThrows()
    {
        // thrown for null too: the declaration is wrong, not the value
        var ex = Assert.Throws<XamlException>(() => XamlServices.Write(new CDataWrongType()));
        Assert.Contains("'CDataWrongType.Value'", ex.Message);
    }
}
