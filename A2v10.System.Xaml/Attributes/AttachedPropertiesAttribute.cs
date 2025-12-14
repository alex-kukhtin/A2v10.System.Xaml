// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.System.Xaml;

[AttributeUsage(AttributeTargets.Class)]
public class AttachedPropertiesAttribute(String list) : Attribute
{
    public String List { get; } = list;
}
