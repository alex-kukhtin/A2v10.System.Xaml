﻿
namespace A2v10.System.Xaml;

public class XamlTextContent : XamlNode
{
	public XamlTextContent() 
		: base(String.Empty)
	{
	}

	public String? Text { get; set; }
}
