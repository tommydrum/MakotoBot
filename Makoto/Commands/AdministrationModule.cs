using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Makoto.Commands
{
    [Name("Administrator Commands")]
    public class AdministrationModule : ModuleBase
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        public AdministrationModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;
        }

        [Command("reload"), Summary("Reloads the modules for this bot. Admin only")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task Restart()
        {
            await ReplyAsync("Unloading modules");

            var modulecopy = _service.Modules.ToList();
            foreach (var module in modulecopy)
            {
                await _service.RemoveModuleAsync(module);
            }

            await ReplyAsync("Loading modules");
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());

            string response = "Modules loaded: ```\n";

            foreach (var module in _service.Modules)
            {
                response += $"{module.Name}\n";
            }

            response += "```";

            await ReplyAsync(response);
        }

        [Command("listmodules"), Summary("List all active modules. Admin only")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task ListModules()
        {
            string response = "Modules loaded: ```\n";

            foreach (var module in _service.Modules)
            {
                response += $"{module.Name}\n";
            }

            response += "```";

            await ReplyAsync(response);
        }

        [Command("stop"), Summary("Stop's the bot. Admin only")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task Stop()
        {
            await ReplyAsync("Stopping server.");
            Environment.Exit(0);
        }
    }
}
