// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;

namespace A2v10.System.Xaml
{
	public class TypeDescriptorCache
	{
		private readonly ConcurrentDictionary<ClassNamePair, TypeDescriptor> _descriptorCache = new();

		public TypeDescriptor GetOrAdd(ClassNamePair key, Func<ClassNamePair, TypeDescriptor> valueFactory)
		{
			return _descriptorCache.GetOrAdd(key, valueFactory);
		}
	}
}
