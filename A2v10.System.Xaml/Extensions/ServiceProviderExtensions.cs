
// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.System.Xaml;

public static class ServiceProviderExtensions
{
    public static T GetRequiredXamlService<T>(this IServiceProvider sp)
    {
        var service = sp.GetService(typeof(T))
            ?? throw new InvalidOperationException($"Service '{typeof(T)}' is not registered.");
        return (T) service;
    }
    public static T? GetXamlService<T>(this IServiceProvider sp)
    {
        var service = sp.GetService(typeof(T));
        if (service == null) 
            return default;
        return (T)service;
    }
}
