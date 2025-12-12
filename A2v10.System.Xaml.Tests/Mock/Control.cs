// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests.Mock;

public class Control : UIElementBase
{
	public Button? Button { get; set; }	
}

public class Selector : Control
{
	public String? Text { get; init; }
}
