// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.System.Xaml
{
	public abstract class MarkupExtension
	{
		protected MarkupExtension()
		{
		}

		public abstract Object ProvideValue(IServiceProvider serviceProvider);
	}
}
