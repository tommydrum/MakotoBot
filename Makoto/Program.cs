using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Net.Providers.WS4Net;
using Makoto.Commands;
using Discord.Addons.CommandCache;


/*   
 *   This bot requries a JSON file called _configuration.json to be located in the same directory as the executable.
 *   In that file, define discord:token and discord:prefix.
 *   token is the bot authentication token.
 *   prefix is the role prefix that will get prepended to role name when dealing with the RoleManager.
 */

namespace Makoto
{
    public class Program
    {
        private DiscordSocketClient _client;

        // Keep the CommandService and IServiceCollection around for use with commands.
        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly CommandService _commands = new CommandService();
        public IConfigurationRoot Configuration { get; set; }
        private IServiceProvider _services;

        public static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // Configuration file management:
            var builder = new ConfigurationBuilder()        // Create a new instance of the config builder
                .SetBasePath(AppContext.BaseDirectory)      // Specify the default location for the config file
                .AddJsonFile("_configuration.json");        // Add this (json encoded) file to the configuration
            Configuration = builder.Build();                // Build the configuration

            // Discord Client creation and customization
            _client = new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    AlwaysDownloadUsers = true,
                    HandlerTimeout = null,
                });

            // Insert log
            _client.Log += Log;

            // Centralize the logic for commands into a seperate method.
            await InitCommands();

            // Log in and start the bot's service.
            await _client.LoginAsync(TokenType.Bot, Configuration["discord:token"]);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);

        }

        private Task Log(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message}");
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        private async Task InitCommands()
        {

            // Insert services/dependencies for commands
            _map.AddSingleton(Configuration);
            _map.AddSingleton(_client);
            _map.AddSingleton(new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false

            }));
            _map.AddSingleton(new CommandCacheService(_client));

            // Build the service provider.
            _services = _map.BuildServiceProvider();

            // Insert all command modules (searching via command assembly)
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;
            // Searching to see if message is mentioning the bot
            if (msg.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed succesfully).
                var result = await _commands.ExecuteAsync(context, pos, _services);

                // Unknown command
                if (!result.IsSuccess && result.Error == CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync("Unkown command. Use `help` for a list of commands");
                //Actual problem
                else if (!result.IsSuccess)
                    await msg.Channel.SendMessageAsync("There was a problem: ```\n" + result.ErrorReason + "```");
            }
        }
    }
}