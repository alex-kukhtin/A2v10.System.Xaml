// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.System.Xaml
{
	[AttributeUsage(AttributeTargets.Class)]
	public class AttachedPropertiesAttribute : Attribute
	{
		public String List { get; }

		public AttachedPropertiesAttribute(String list)
		{
			List = list;
		}
	}
}
