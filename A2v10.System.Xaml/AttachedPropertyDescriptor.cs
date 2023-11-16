// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

using System.ComponentModel;

namespace A2v10.System.Xaml;
public record AttachedPropertyDescriptor(Type PropertyType, Action<Object, Object> Lambda, TypeConverter? TypeConverter);
