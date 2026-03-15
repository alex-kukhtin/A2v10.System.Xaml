// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.System.Xaml;

public class XamlServiceProvider : IServiceProvider
{
	private readonly Dictionary<Type, Object> _services = [];
	private readonly XamlProvideValueTarget _provideValueTarget = new();
	private readonly XamlRootObjectProvider _rootObjectProvider = new();
	private readonly XamlAttachedPropertyManager _attachedPropertyManager = new();
	private readonly XamlUriContext _uriContext = new();


	public XamlServiceProvider(Boolean useInternalAttached = false)
	{
		AddService<IProvideValueTarget>(_provideValueTarget);
		AddService<IRootObjectProvider>(_rootObjectProvider);
		if (!useInternalAttached)
			AddService<IAttachedPropertyManager>(_attachedPropertyManager);
		AddService<IUriContext>(_uriContext);
	}

	public void AddService<T>(Object service)
	{
		_services.Add(typeof(T), service);
	}

	public void AddService(Type serviceType, Object service)
	{
		_services.Add(serviceType, service);
	}

	public Object? GetService(Type serviceType)
	{
		if (_services.TryGetValue(serviceType, out Object? service))
			return service;
		return null;
	}

	public T? GetService<T>()
	{
		if (_services.TryGetValue(typeof(T), out Object? service))
			return (T) service;
		return default;
	}

	public XamlProvideValueTarget ProvideValueTarget => _provideValueTarget;

	public void SetRoot(Object root)
	{
		_rootObjectProvider.RootObject = root;
	}

}
