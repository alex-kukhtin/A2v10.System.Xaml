// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Xml;

namespace A2v10.System.Xaml
{
	public class XamlReaderService : IXamlReaderService
	{
		private readonly TypeDescriptorCache _typeDescriptorCache = new();

		public virtual XamlServicesOptions Options { get; set; }

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

		public Object Load(Stream stream, Uri baseUri = null)
		{
			var xaml = new XamlReader(XmlReader.Create(stream, _settings), baseUri, _typeDescriptorCache, Options);
			Options?.OnCreateReader?.Invoke(xaml);
			return xaml.Read();
		}

		public Object Load(XmlReader rdr, Uri baseUri = null)
		{
			var xaml = new XamlReader(rdr, baseUri, _typeDescriptorCache, Options);
			Options?.OnCreateReader?.Invoke(xaml);
			return xaml.Read();
		}
	}
}
