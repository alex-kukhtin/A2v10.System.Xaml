// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ContentPropertyAttribute(String name) : Attribute
{
    public String Name { get; } = name;
}
