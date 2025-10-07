#if NETSTANDARD

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

public static class TypeExtensions
{
    public static bool IsAssignableTo(this Type type, Type targetType)
    {
        return targetType.IsAssignableFrom(type);
    }
}

#endif
