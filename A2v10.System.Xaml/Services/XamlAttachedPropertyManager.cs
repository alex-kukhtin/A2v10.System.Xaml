// Copyright © 2021 Alex Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public class XamlAttachedPropertyManager : IAttachedPropertyManager
{

	public record PropertyDef(String Name, Object Value);

	private readonly Dictionary<PropertyDef, Object> _map = new();

	public void SetProperty(String propName, Object obj, Object value)
	{
		_map.Add(new PropertyDef(propName, obj), value);
	}

	public T? GetProperty<T>(String propName, Object obj)
	{
		if (_map.TryGetValue(new PropertyDef(propName, obj), out Object? value))
			return (T?) PropertyConvertor.ConvertValue(value, typeof(T), null);
		return default;
	}
}

