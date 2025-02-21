using A2v10.System.Xaml.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.System.Xaml.Tests
{
	[TestClass]
	[TestCategory("Xaml.Read.Simple")]
	public class SimpleRead
	{
		[TestMethod]
		public void SimpleProperty()
		{
			string xaml = @"
<Sequence xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	Ref=""Ref0"">
</Sequence>
";
			var obj = XamlServices.Parse(xaml, XamlServicesOptions.BpmnXamlOptions);

			Assert.AreEqual(typeof(Sequence), obj.GetType());
			var sq = obj as Sequence;
			Assert.IsNotNull(sq);
			Assert.AreEqual("Ref0", sq.Ref);
		}

		[TestMethod]
		public void SimpleWithChildren()
		{
			string xaml = @"
<Sequence xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" xmlns:sys=""clr-namespace:System;assembly=mscorlib"" 
	Ref=""Ref0"">
<Sequence.Activities>
	<Code Ref=""Ref1"" Script=""X = X + 1""/>
	<Code Ref=""Ref2"" Script=""X = X + 2""/>
	<Code Ref=""Ref3"" Script=""X = X + 3""/>
</Sequence.Activities>
</Sequence>
";
			var obj = XamlServices.Parse(xaml);

			Assert.AreEqual(typeof(Sequence), obj.GetType());
			var sq = obj as Sequence;
			Assert.IsNotNull(sq);
			Assert.IsNotNull(sq.Activities);
			Assert.AreEqual(3, sq.Activities.Count);
			var code0 = sq.Activities[0] as Code;
			Assert.IsNotNull(code0);
			Assert.AreEqual("Ref1", code0.Ref);
			Assert.AreEqual("X = X + 1", code0.Script);
			var code2 = sq.Activities[2] as Code;
			Assert.IsNotNull(code2);
			Assert.AreEqual("Ref3", code2.Ref);
			Assert.AreEqual("X = X + 3", code2.Script);
		}

		[TestMethod]
		public void ContentText()
		{
			string xaml = @"
<Code xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" xmlns:sys=""clr-namespace:System;assembly=mscorlib"" 
		Ref=""Ref0"">
	Code Text 1
</Code>
";
			var obj = XamlServices.Parse(xaml, XamlServicesOptions.BpmnXamlOptions);

			Assert.AreEqual(typeof(Code), obj.GetType());
			var code0 = obj as Code;
			Assert.IsNotNull(code0);
			Assert.AreEqual("Ref0", code0.Ref);
			Assert.AreEqual("\n\tCode Text 1\n", code0.Script);
		}

		[TestMethod]
		public void ContentCollection()
		{
			string xaml = @"
<Sequence xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" xmlns:sys=""clr-namespace:System;assembly=mscorlib"" 
		Ref=""Ref0"">
	<Code Ref=""Ref1"" Script=""X = 1""/>
	<Code Ref=""Ref2"" Script=""X = 2""/>
	<Code />
</Sequence>
";
			var obj = XamlServices.Parse(xaml, XamlServicesOptions.BpmnXamlOptions);

			Assert.AreEqual(typeof(Sequence), obj.GetType());
			var sq = obj as Sequence;
			Assert.IsNotNull(sq);
			Assert.IsNotNull(sq.Activities);
			Assert.AreEqual(3, sq.Activities.Count);
			var code0 = sq.Activities[0] as Code;
			Assert.IsNotNull(code0);
			Assert.AreEqual("Ref1", code0.Ref);
			Assert.AreEqual("X = 1", code0.Script);
			var code2 = sq.Activities[1] as Code;
			Assert.IsNotNull(code2);
			Assert.AreEqual("Ref2", code2.Ref);
			Assert.AreEqual("X = 2", code2.Script);
		}


		[TestMethod]
		public void ContentWithColors()
		{
            string xaml = @"
<Sequence 
	xmlns=""clr-namespace:A2v10.System.Xaml.Tests.Mock;assembly=A2v10.System.Xaml.Tests"" 
	xmlns:color=""http://www.omg.org/spec/BPMN/non-normative/color/1.0""
	xmlns:bioc=""http://bpmn.io/schema/bpmn/biocolor/1.0"" 
>
  <Code bioc:stroke=""#6b3c00"" bioc:fill=""#ffe0b2"" color:background-color=""#ffe0b2""></Code>
</Sequence>
";

            var obj = XamlServices.Parse(xaml, XamlServicesOptions.BpmnXamlOptions);

			int z = 55;
        }
    }
}
