// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Xml;

namespace A2v10.System.Xaml;
public interface IXamlReaderService
{
	Object ParseXml(String xml);
    Object Load(Stream stream, Uri? baseUri = null);
    Object Load(XmlReader rdr, Uri? baseUri = null);
    T ParseXml<T>(String xml) where T : class;
    T Load<T>(Stream stream, Uri? baseUri = null) where T : class;
    T Load<T>(XmlReader rdr, Uri? baseUri = null) where T : class;

    XamlServicesOptions? Options { get; }
}

