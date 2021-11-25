using System;

namespace A2v10.System.Xaml.Tests.Mock
{

	public enum VariableType
	{
		String,
		Number
	}

	public class Variable
	{
		public String? Name { get; set; }
		public VariableType Type { get; set; }
	}
}
