// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public abstract class MarkupExtension
{
	protected MarkupExtension()
	{
	}

	public abstract Object? ProvideValue(IServiceProvider serviceProvider);
}

