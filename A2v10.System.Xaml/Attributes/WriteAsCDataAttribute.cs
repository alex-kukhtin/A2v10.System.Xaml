// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.System.Xaml;

/* XamlWriter writes the String property as a CDATA element, not as an attribute: in an attribute
 * line breaks and tabs turn into escapes (&#xA;). The text is kept as is, except line endings:
 * the reader normalizes CRLF to LF. A value with ']]>' is an error: CDATA cannot hold it.
 */
[AttributeUsage(AttributeTargets.Property)]
public sealed class WriteAsCDataAttribute() : Attribute
{
}
