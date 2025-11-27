// Copyright © 2021-2023 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Xml;

namespace A2v10.System.Xaml.Tests;

public static class XamlServices
{
	public static Object Parse(String xaml, XamlServicesOptions? options = null)
	{
		var xamlReader = new XamlReaderService()
		{
			Options = options
		};
		return xamlReader.ParseXml(xaml);
	}

	public static Object Load(Stream stream, XamlServicesOptions? options = null)
	{
		var xamlReader = new XamlReaderService()
		{
			Options = options
		};
		return xamlReader.Load(stream);
	}

	public static Object Load(XmlReader rdr, XamlServicesOptions? options = null)
	{
		var xamlReader = new XamlReaderService()
		{
			Options = options
		};
		return xamlReader.Load(rdr);
	}
}
