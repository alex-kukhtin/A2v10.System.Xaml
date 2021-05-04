﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.System.Xaml
{
	public record NamespaceDef(String Name, Boolean IsCamelCase, String Namespace, String Assembly, Boolean IsSkip);

	public class XamlServicesOptions
	{
		public NamespaceDef[] Namespaces { get; init; }
		public Dictionary<String, String> Aliases { get; init; }
		public Boolean DisableMarkupExtensions { get; init; }

		public Action<XamlReader> OnCreateReader { get; init; }

		private static readonly NamespaceDef[] BPMNNamespaces = new NamespaceDef[] {
			new NamespaceDef("http://www.omg.org/spec/bpmn/20100524/model", true, "A2v10.Workflow.Bpmn", "A2v10.Workflow", false),
			new NamespaceDef("http://www.omg.org/spec/bpmn/20100524/di", false, "A2v10.Workflow.Bpmn.Diagram", "A2v10.Workflow", true),
			new NamespaceDef("http://www.omg.org/spec/dd/20100524/di", false, "A2v10.Workflow.Bpmn.Diagram", "A2v10.Workflow", true),
			new NamespaceDef("http://www.omg.org/spec/dd/20100524/dc", false, "A2v10.Workflow.Bpmn.Diagram", "A2v10.Workflow", true),
			new NamespaceDef("http://www.w3.org/2001/xmlschema-instance", true, "A2v10.Workflow.Bpmn", "A2v10.Workflow", false),
		};

		public static XamlServicesOptions BpmnXamlOptions =>
			new()
			{
				Namespaces = BPMNNamespaces,
				Aliases = new Dictionary<String, String>() {
					{ "Task", "BpmnTask" }
				},
				DisableMarkupExtensions = true
			};
	}
}
