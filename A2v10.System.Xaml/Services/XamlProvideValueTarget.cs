﻿// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public class XamlProvideValueTarget : IProvideValueTarget
{
	public Object? TargetObject { get; set; }

	public Object? TargetProperty { get; set; }
}

