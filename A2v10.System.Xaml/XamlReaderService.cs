// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Xml;

namespace A2v10.System.Xaml;
public class XamlReaderService : IXamlReaderService
{
	private readonly TypeDescriptorCache _typeDescriptorCache = new();

	public virtual XamlServicesOptions? Options { get; set; }

	private static readonly XmlReaderSettings _settings = new()
	{
		IgnoreComments = true,
		IgnoreWhitespace = false
	};

	public Object ParseXml(String xml)
	{
		using var stringReader = new StringReader(xml);
		using var xmlrdr = XmlReader.Create(stringReader, _settings);
		return Load(xmlrdr);
	}
    public T ParseXml<T>(String xml) where T : class
    {
        using var stringReader = new StringReader(xml);
        using var xmlrdr = XmlReader.Create(stringReader, _settings);
        return Load<T>(xmlrdr);
    }

    public Object Load(Stream stream, Uri? baseUri = null)
	{
		var xaml = new XamlReader(XmlReader.Create(stream, _settings), baseUri, _typeDescriptorCache, Options);
		Options?.OnCreateReader?.Invoke(xaml);
		return xaml.Read() ?? throw new XamlException("Load failed (Stream)");
	}
    public T Load<T>(Stream stream, Uri? baseUri = null) where T: class
    {
        var xaml = new XamlReader(XmlReader.Create(stream, _settings), baseUri, _typeDescriptorCache, Options);
        Options?.OnCreateReader?.Invoke(xaml);
        return xaml.Read<T>() ?? throw new XamlException("Load failed (Stream)");
    }

    public Object Load(XmlReader rdr, Uri? baseUri = null)
	{
		var xaml = new XamlReader(rdr, baseUri, _typeDescriptorCache, Options);
		Options?.OnCreateReader?.Invoke(xaml);
		return xaml.Read() ?? throw new XamlException("Load fialed (XmlReader)");
	}

    public T Load<T>(XmlReader rdr, Uri? baseUri = null) where T: class
    {
        var xaml = new XamlReader(rdr, baseUri, _typeDescriptorCache, Options);
        Options?.OnCreateReader?.Invoke(xaml);
        return xaml.Read<T>() ?? throw new XamlException("Load fialed (XmlReader)");
    }
}

