// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests.Mock;

public class Control : UIElementBase
{
	public Button? Button { get; set; }	
}

public class Selector : Control
{
    public String? Delegate { get; set; }
    public String? SetDelegate { get; set; }
    public String? Fetch { get; set; }
    public String? FetchData { get; set; }
    public String? DisplayProperty { get; set; }
    public String? Text { get; init; }
}


[IgnoreWriteProperties("Delegate,SetDelegate,Fetch,FetchData,DisplayProperty")]
public class SelectorSimple : Selector
{
    public String? Url { get; set; }
    public String? Data { get; set; }
    public Boolean Folder { get; set; }
}
