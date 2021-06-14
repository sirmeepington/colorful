using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using System.Collections.Generic;
using System.Text;

namespace Colorful.Discord
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected StringBuilder _strBuilder;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _strBuilder = new StringBuilder();
            _strBuilder.AppendLine("```yaml");
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _strBuilder.AppendLine($"{command.Name,5} :: {command.Description}");

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            foreach (var cmd in cmds)
            {
                _strBuilder.AppendLine($"{cmd.Name,5} :: {cmd.Description}");
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            _strBuilder.AppendLine("```");
            return new CommandHelpMessage(content: _strBuilder.ToString());
        }
    }
}
