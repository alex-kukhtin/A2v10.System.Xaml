// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public sealed class XamlException : Exception
{
    public XamlException(String? message) : base(message)
    {
    }

    public XamlException(String? message, Exception? innerException) : base(message, innerException)
    {
    }
}

