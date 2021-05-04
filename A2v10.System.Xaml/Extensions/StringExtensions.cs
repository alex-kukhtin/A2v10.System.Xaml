﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml
{
	public static class StringExtensions
	{
		public static String ToPascalCase(this String str)
		{
			if (str == null || str.Length < 1)
				return str;
			if (Char.IsLower(str[0]))
				return Char.ToUpper(str[0]) + str[1..];
			return str;
		}
	}
}
