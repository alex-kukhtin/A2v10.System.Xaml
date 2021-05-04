// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml;

namespace A2v10.System.Xaml
{
	public class XamlReader
	{
		private readonly XamlNode _root = new() { Name = "Root" };
		private readonly XmlReader _rdr;
		private readonly XamlServicesOptions _options;
		private readonly XamlServiceProvider _xamlServiceProvider;
		private readonly TypeDescriptorCache _typeCache;

		private readonly Stack<XamlNode> _elemStack = new();

		public XamlReader(XmlReader rdr, Uri baseUri, TypeDescriptorCache typeCache, XamlServicesOptions options)
		{
			_rdr = rdr;
			_typeCache = typeCache;
			_options = options;

			_elemStack.Push(_root);
			_xamlServiceProvider = new XamlServiceProvider();
			if (baseUri != null)
			{
				var uriContext = _xamlServiceProvider.GetService<IUriContext>();
				if (uriContext != null)
					uriContext.BaseUri = baseUri;
			}
		}

		public Object Read()
		{
			var nodeBuilder = new NodeBuilder(_xamlServiceProvider, _typeCache, _options);
			while (_rdr.Read())
			{
				if (_rdr.NodeType == XmlNodeType.Comment || _rdr.NodeType == XmlNodeType.Whitespace)
					continue;
				ReadNode(nodeBuilder);
			}
			if (!_root.Children.IsValueCreated)
				return null;
			if (_root.Children.Value.Count == 0)
				return 0;
			else if (_root.Children.Value.Count > 1)
				throw new XamlException("Invalid Xaml structure");
			var r = _root.Children.Value[0];
			var node = nodeBuilder.BuildNode(r);
			if (node == null)
				throw new XamlException("Root node not found");
			_xamlServiceProvider.SetRoot(node);
			nodeBuilder.ExecuteDeferred();
			return node;
		}

		void ReadNode(NodeBuilder builder)
		{
			switch (_rdr.NodeType)
			{
				case XmlNodeType.Element:
					{
						var node = new XamlNode()
						{
							Name = _rdr.Name
						};
						StartNode(node);
						if (_rdr.IsEmptyElement)
							EndNode(builder);
						ReadAttributes(node, builder);
					}
					break;
				case XmlNodeType.EndElement:
					EndNode(builder);
					break;
				case XmlNodeType.Text:
					AddContent();
					break;
				case XmlNodeType.XmlDeclaration:
					AddDeclaration();
					break;
			}
		}

		void ReadAttributes(XamlNode node, NodeBuilder builder)
		{
			for (var i = 0; i < _rdr.AttributeCount; i++)
			{
				_rdr.MoveToAttribute(i);
				if (_rdr.Name.StartsWith("xmlns"))
				{
					var prefix = _rdr.Name[5..];
					if (prefix.StartsWith(':'))
						prefix = prefix[1..];
					builder.AddNamespace(prefix, _rdr.Value);
				}
				else
				{
					node.AddProperty(builder, _rdr.Name, _rdr.Value);
				}
			}
		}

		void StartNode(XamlNode node)
		{
			_elemStack.Push(node);
		}

		void AddContent()
		{
			var node = _elemStack.Peek();
			node.SetContent(_rdr.Value); // preserve whitespace
		}

		void EndNode(NodeBuilder builder)
		{
			var ch = _elemStack.Pop();
			var parent = _elemStack.Peek();
			parent.AddChildren(ch, builder);
		}

		static void AddDeclaration()
		{
		}

		public void InjectService<T>(T service)
		{
			_xamlServiceProvider.AddService<T>(service);
		}

	}
}
