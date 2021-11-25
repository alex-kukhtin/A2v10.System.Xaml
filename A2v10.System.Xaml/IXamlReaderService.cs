// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System.IO;
using System.Xml;

namespace A2v10.System.Xaml;
public interface IXamlReaderService
{
	Object ParseXml(String xml);
	Object Load(Stream stream, Uri? baseUri = null);
	Object Load(XmlReader rdr, Uri? baseUri = null);

	XamlServicesOptions? Options { get; }
}

