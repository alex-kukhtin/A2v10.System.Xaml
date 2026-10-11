// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace A2v10.System.Xaml;

public class TypeDescriptorCache
{
	private readonly ConcurrentDictionary<ClassNamePair, TypeDescriptor> _descriptorCache = new();
	public TypeDescriptor GetOrAdd(ClassNamePair key, Func<ClassNamePair, TypeDescriptor> valueFactory)
	{
		return _descriptorCache.GetOrAdd(key, valueFactory);
	}

	internal Boolean TryGet(ClassNamePair key, [NotNullWhen(true)] out TypeDescriptor? descriptor)
	{
		return _descriptorCache.TryGetValue(key, out descriptor);
	}
}
