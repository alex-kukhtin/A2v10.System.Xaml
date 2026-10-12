// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.System.Xaml;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AttachedPropertiesAttribute(String list) : Attribute
{
    public String List { get; } = list;
}


[AttributeUsage(AttributeTargets.Class)]
public sealed class AttachedTransparentAttribute() : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class AttachedPropertyTypeAttribute(String name, String type) : Attribute
{
    public String Name { get; } = name;
    public String Type { get; } = type; 
}
