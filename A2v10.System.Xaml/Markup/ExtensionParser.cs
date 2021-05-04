﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.System.Xaml
{
	public class ExtensionParser
	{
		public static XamlNode Parse(NodeBuilder builder, String text)
		{
			return (new ExtensionParser(builder, text)).Parse();
		}

		enum TokenType
		{
			Undefined,
			Comma,
			LeftCurly,
			RightCurly,
			Equal,
			Ider,
			String
		}

		enum State
		{
			Start,
			Name,
			Value,
			Equal,
			Continue,
			End
		}

		const Char NULL_CHAR = '\0';

		private readonly NodeBuilder _builder;
		private readonly String _text;
		private readonly Int32 _len;
		
		Int32 _pos;
		Char _ch;

		private XamlNode _node;

		TokenType _tokenType;
		Int32 _tokenStart;
		Int32 _tokenLen;

		private State _state;
		String _propertyName;

		String TokenValue => _text.Substring(_tokenStart, _tokenLen);

		private ExtensionParser(NodeBuilder builder, String text)
		{
			_builder = builder;
			_text = text;
			_len = _text.Length;
			ReadChar();
		}

		XamlNode Parse()
		{
			if (_ch != '{')
				throw new XamlException($"Invalid Xaml extension '{_text}'");
			NextChar();
			Read();
			return _node;
		}

		void Read()
		{
			while (_ch != NULL_CHAR)
			{
				NextToken();
				DoStep();
				if (_tokenType == TokenType.RightCurly)
					return;
			}
		}


		void DoStep()
		{
			switch (_state)
			{
				case State.Start:
					if (_tokenType == TokenType.Ider)
					{
						_node = new XamlNode() { Name = TokenValue };
						_state = State.Name;
					} 
					else
						throw new XamlException($"Invalid token type for State 'Start'. Expected: 'Ider', actual: '{_tokenType}', value: {TokenValue}");
					break;
				case State.Name:
					if (_tokenType == TokenType.Ider)
						_propertyName = TokenValue;
					_state = State.Equal;
					break;
				case State.Equal:
					if (_tokenType == TokenType.Equal)
						_state = State.Value;
					else if (_tokenType == TokenType.RightCurly)
					{
						_node.AddConstructorArgument(TokenValue);
						_state = State.End;
					}
					else if (_tokenType == TokenType.Comma)
					{
						// argument
						_node.AddConstructorArgument(TokenValue);
						_state = State.Name;
					}
					break;
				case State.Value:
					if (_tokenType == TokenType.String || _tokenType == TokenType.Ider)
						_node.AddProperty(_builder, _propertyName, TokenValue);
					_state = State.Continue;
					break;
				case State.Continue:
					if (_tokenType == TokenType.Comma)
						_state = State.Name;
					else
						_state = State.End;
					break;
			}
		}

		void NextToken()
		{
			while (Char.IsWhiteSpace(_ch) && _ch != NULL_CHAR)
				NextChar();
			Int32 tokPos = _pos;
			Int32 cbc;
			switch (_ch)
			{
				case ',':
					_tokenType = TokenType.Comma;
					NextChar();
					break;
				case '=':
					_tokenType = TokenType.Equal;
					NextChar();
					break;
				case '}':
					_tokenType = TokenType.RightCurly;
					NextChar();
					break;
				case '{':
					_tokenType = TokenType.String;
					cbc = 0;
					while (cbc >= 0)
					{
						while (_ch != NULL_CHAR)
						{
							if (_ch == '{')
								cbc++;
							else if (_ch == '}')
							{
								cbc--;
								NextChar();
								if (cbc == 0)
								{
									_tokenStart = tokPos;
									_tokenLen = _pos - tokPos;
									return;
								}
							}
							NextChar();
						}
					}
					break;
				case '\'':
					_tokenType = TokenType.String;
					NextChar();
					while (_ch != '\'' && _ch != NULL_CHAR)
						NextChar();
					NextChar();
					_tokenStart = tokPos + 1;
					_tokenLen = _pos - tokPos - 2;
					break;
				default:
					_tokenType = TokenType.Ider;
					while (!Char.IsWhiteSpace(_ch) && _ch != ',' && _ch != '=' && _ch != '}' && _ch != '{' && _ch != NULL_CHAR)
						NextChar();
					_tokenStart = tokPos;
					_tokenLen = _pos - tokPos;
					break;
			}
		}

		void NextChar()
		{
			if (_pos < _len) 
				_pos++;
			ReadChar();
		}

		void ReadChar()
		{
			_ch = _pos < _len ? _text[_pos] : NULL_CHAR;
		}
	}
}
