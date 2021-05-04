// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.ComponentModel;

namespace A2v10.System.Xaml
{
	public record AttachedPropertyDescriptor
	{
		public Type PropertyType { get; set; }
		public Action<Object, Object> Lambda { get; init; }
		public TypeConverter TypeConverter { get; set; }
	}
}
