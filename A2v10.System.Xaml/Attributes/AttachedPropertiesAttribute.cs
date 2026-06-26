// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.System.Xaml;

[AttributeUsage(AttributeTargets.Class)]
public class AttachedPropertiesAttribute(String list) : Attribute
{
    public String List { get; } = list;
}


[AttributeUsage(AttributeTargets.Class)]
public class AttachedTransparentAttribute() : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class AttachedPropertyTypeAttribute(String name, String type) : Attribute
{
    public String Name { get; } = name;
    public String Type { get; } = type; 
}
