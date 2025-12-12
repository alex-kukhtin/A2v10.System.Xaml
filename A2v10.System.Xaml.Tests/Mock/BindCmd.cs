using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock
{
	public enum CommandType
	{
		None,
		Execute,
		Run
	}

	public class BindCmd : BindBase
	{
		public CommandType? Command { get; set; }
		public String? Argument { get; set; }
		public String? CommandName { get; set; }

		public BindCmd()
		{

		}

		public BindCmd(String command)
		{
			Command = (CommandType) Enum.Parse<CommandType>(command);
		}
	}
}
