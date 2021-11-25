// Copyright © 2021 Alex Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public interface IAttachedPropertyManager
{
	void SetProperty(String propName, Object obj, Object value);
	T? GetProperty<T>(String propName, Object obj);
}

