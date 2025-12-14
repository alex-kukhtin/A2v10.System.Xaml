// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests.Mock;

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

        public override String CreateMarkup()
        {
            return $$"""{BindCmd {{Command}}}""";
        }
    }
