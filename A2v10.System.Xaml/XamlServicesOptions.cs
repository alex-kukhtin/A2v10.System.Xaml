// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml;
public record NamespaceDef(String Name, Boolean IsCamelCase, String Namespace, String Assembly, Boolean IsSkip);

public class XamlServicesOptions(NamespaceDef[] namespaces)
{
    public NamespaceDef[] Namespaces { get; } = namespaces;
    public Dictionary<String, String>? Aliases { get; init; }
	public Boolean DisableMarkupExtensions { get; init; }
	public Boolean SkipUnknownProperties { get; init; }	
    public Action<XamlReader>? OnCreateReader { get; init; }

	private static readonly NamespaceDef[] BPMNNamespaces = [
		new("http://www.omg.org/spec/bpmn/20100524/model", true, "A2v10.Workflow.Bpmn", "A2v10.Workflow", false),
		new("http://www.omg.org/spec/bpmn/20100524/di", false, "A2v10.Workflow.Bpmn.Diagram", "A2v10.Workflow", true),
		new("http://www.omg.org/spec/dd/20100524/di", false, "A2v10.Workflow.Bpmn.Diagram", "A2v10.Workflow", true),
		new("http://www.omg.org/spec/dd/20100524/dc", false, "A2v10.Workflow.Bpmn.Diagram", "A2v10.Workflow", true),
		new("http://www.w3.org/2001/xmlschema-instance", true, "A2v10.Workflow.Bpmn", "A2v10.Workflow", false),
        new("http://bpmn.io/schema/bpmn/biocolor/1.0", false, "", "Self", true),
        new("http://www.omg.org/spec/bpmn/non-normative/color/1.0", false, "", "Self", true)
    ];

	public static XamlServicesOptions BpmnXamlOptions =>
		new(BPMNNamespaces)
		{
			Aliases = new Dictionary<String, String>() {
				{ "Task", "BpmnTask" }
			},
			DisableMarkupExtensions = true,
            SkipUnknownProperties = true
        };
}

