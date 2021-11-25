// Copyright © 2021 Alex Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public interface IProvideValueTarget
{
	Object? TargetObject { get; }
	Object? TargetProperty { get; }
}

