using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        [Summary("Lists available commands.")]
        public async Task HelpAsync()
        {
            List<Embed> embeds = new();
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use:",
            };

            var mgr = SysCordSettings.Manager;
            var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
            var owner = app.Owner.Id;
            var uid = Context.User.Id;

            foreach (var module in _service.Modules)
            {
                string? description = null;
                HashSet<string> mentioned = new();
                foreach (var cmd in module.Commands)
                {
                    var name = cmd.Name;
                    if (mentioned.Contains(name))
                        continue;
                    if (cmd.Attributes.Any(z => z is RequireOwnerAttribute) && owner != uid)
                        continue;
                    if (cmd.Attributes.Any(z => z is RequireSudoAttribute) && !mgr.CanUseSudo(uid))
                        continue;

                    mentioned.Add(name);
                    var result = await cmd.CheckPreconditionsAsync(Context).ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        description = cmd.Aliases[0] + "\n";
                    }
                }
                if (string.IsNullOrWhiteSpace(description))
                    continue;

                var moduleName = module.Name;
                var gen = moduleName.IndexOf('`');
                if (gen != -1)
                    moduleName = moduleName[..gen];

                if (builder.Fields.Count == 25)
                {
                    embeds.Add(builder.Build());
                    builder.Fields.Clear();
                    builder.Description = string.Empty;
                }

                builder.AddField(x =>
                {
                    x.Name = moduleName;
                    x.Value = description;
                    x.IsInline = false;
                });
            }

            if (builder.Fields.Count > 0)
                embeds.Add(builder.Build());

            await ReplyAsync("Help has arrived!", embed: embeds.FirstOrDefault()).ConfigureAwait(false);
        }
    }
}
