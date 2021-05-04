// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.System.Xaml
{
	public class XamlProvideValueTarget : IProvideValueTarget
	{
		public Object TargetObject { get; set; }

		public Object TargetProperty { get; set; }
	}
}
