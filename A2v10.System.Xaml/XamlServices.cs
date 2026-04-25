// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Xml;

namespace A2v10.System.Xaml;

public static class XamlServices
{
	private static readonly TypeDescriptorCache _typeDescriptorCache = new();
    public static Object Parse(String xaml, XamlServicesOptions? options = null)
	{
		var xamlReader = new XamlReaderService(_typeDescriptorCache)
		{
			Options = options
		};
		return xamlReader.ParseXml(xaml);
	}

	public static Object Load(Stream stream, XamlServicesOptions? options = null)
	{
		var xamlReader = new XamlReaderService(_typeDescriptorCache)
		{
			Options = options
		};
		return xamlReader.Load(stream);
	}

	public static Object Load(XmlReader rdr, XamlServicesOptions? options = null)
	{
		var xamlReader = new XamlReaderService(_typeDescriptorCache)
		{
			Options = options
		};
		return xamlReader.Load(rdr);
	}

    public static T Parse<T>(String xaml, XamlServicesOptions? options = null) where T: class
    {
		return Parse(xaml, options) as T
				?? throw new InvalidOperationException($"Expected {typeof(T).Name}");
    }

    public static T Load<T>(Stream stream, XamlServicesOptions? options = null) where T: class
    {
        return Load(stream, options) as T
                ?? throw new InvalidOperationException($"Expected {typeof(T).Name}");
    }

    public static T Load<T>(XmlReader rdr, XamlServicesOptions? options = null) where T: class
    {
        return Load(rdr, options) as T
                ?? throw new InvalidOperationException($"Expected {typeof(T).Name}");
    }

    public static String Write(Object obj)
	{
		var xw = new XamlWriter();
		return xw.GetXaml(obj);
	}
}
