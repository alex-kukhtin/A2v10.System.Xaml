// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;

public interface IBindWriter
{
    String? CreateMarkup(String name);
}


public interface IXamlConverter
{
    String? ToXamlString();
}