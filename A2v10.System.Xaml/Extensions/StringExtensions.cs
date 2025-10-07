// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public static class StringExtensions
{
	public static String ToPascalCase(this String? str) {

        if (String.IsNullOrEmpty(str)) 
            return String.Empty;
        Span<Char> buffer = stackalloc Char[str.Length];
        str.AsSpan().CopyTo(buffer);
        buffer[0] = Char.ToUpperInvariant(buffer[0]);
        return new String (buffer);
	}
}

