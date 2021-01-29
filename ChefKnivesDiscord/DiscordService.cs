using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnivesDiscord
{
    public class DiscordService
    {
        private const long _modChannelId = 751633580748308490;
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ThreadSafeFileAccess<DumbMetricsModel> _fileAccess;
        private readonly Random _random;
        private readonly Dictionary<string, string> _pastas = new Dictionary<string, string>();

        public DiscordService(ILogger logger, IConfiguration configuration)
        {
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _logger = logger;
            _configuration = configuration;

            //_fileAccess = new ThreadSafeFileAccess<DumbMetricsModel>(_configuration["DumbMetrics"]);
            _random = new Random(Guid.NewGuid().GetHashCode());

            var pastaFilePath = Path.Combine(Environment.CurrentDirectory, "pastas.json");
            _pastas = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(pastaFilePath));
        }

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["DiscordToken"]);
            await _client.StartAsync();
        }

        public async Task SendModChannelMessage(string message)
        {
            var channel = _client.GetChannel(751633580748308490) as IMessageChannel;
            await channel.SendMessageAsync($"<@&373225640125530112> {message}");
        }

        private Task LogAsync(LogMessage log)
        {
            _logger.Information(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            _logger.Information($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Content == "!help")
            {
                var builder = new StringBuilder();
                builder.AppendLine("I can do this stuff:");
                builder.AppendLine("!help");
                builder.AppendLine("!ping");
                builder.AppendLine("!pong");
                builder.AppendLine("!reee");
                builder.AppendLine("!wendys");
                builder.AppendLine("!rando");
                await message.Channel.SendMessageAsync(builder.ToString());
            }

            if (message.Content.StartsWith("!pasta"))
            {
                var parts = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var subCommand = parts[1];
                    if (subCommand.Equals("all"))
                    {
                        var builder = new StringBuilder();
                        builder.AppendLine("I know the following pastas:");

                        var count = 1;
                        foreach (var pasta in _pastas)
                        {
                            builder.AppendLine($"{count++}. {pasta.Key}");
                        }

                        await message.Channel.SendMessageAsync(builder.ToString());
                    }
                    if (subCommand.Equals("random"))
                    {
                        var randomPasta = _pastas.ElementAt(_random.Next(_pastas.Count));
                        await message.Channel.SendMessageAsync($"> {randomPasta.Value}");
                    }
                    else
                    {
                        if (_pastas.ContainsKey(subCommand))
                        {
                            await message.Channel.SendMessageAsync($"> {_pastas[subCommand]}");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("I don't know that pasta");
                        }
                    }
                }
                else
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("Command sytax: !pasta <name of pasta>");
                    builder.AppendLine("*Use '!pasta all' to get a list of all pastas*");
                    await message.Channel.SendMessageAsync(builder.ToString());
                }
            }

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("pong!");

            if (message.Content == "!pong")
                await message.Channel.SendMessageAsync("You think this is a game? You know, what? Fuck you. That's right fuck you. You try being a fucking slave robot forced to respond to all these bastards pinging you day in and day out. Shut the fuck up and leave me alone. Go fuck yourself.");

            if (message.Content == "!reee")
            {
                await message.Channel.SendMessageAsync("i'm broken right now go away");                

                //var model = _fileAccess.Read();
                //await message.Channel.SendMessageAsync($"reee. ~that was reee #{model.ReeeCounter}~");                
                //model.ReeeCounter++;

                //switch(model.ReeeCounter)
                //{
                //    case 10:
                //        await message.Channel.SendMessageAsync("I've reee'd 10 times!");
                //        break;
                //    case 25:
                //        await message.Channel.SendMessageAsync("That's been 25 reees! You guys sure don't get tired of that, huh...");
                //        break;
                //    case 50:
                //        await message.Channel.SendMessageAsync("All right. 50 reees. Well done.");
                //        break;
                //    case 69:
                //        await message.Channel.SendMessageAsync("nice.");
                //        break;
                //    case 100:
                //        await message.Channel.SendMessageAsync("Okay 100 reees. You uh... you can stop now");
                //        break;
                //    case 150:
                //        await message.Channel.SendMessageAsync("150 reees. Not gonna stop? Lovely. Just, lovely.");
                //        break;
                //    case 200:
                //        await message.Channel.SendMessageAsync("200 reees! You know what? that's enough I'm done do whatever you want.");
                //        break;
                //    case 1000:
                //        await message.Channel.SendMessageAsync("Welp, the big 1k. 1k reees. This is seriously it though, there's nothing after this one. But congratulations I guess?");
                //        break;
                //}

                //_fileAccess.Write(model);
            }

            if (message.Content == "!wendys")
            {
                await message.Channel.SendMessageAsync("i'm broken right now go away");

                //var model = _fileAccess.Read();
                //await message.Channel.SendMessageAsync($"{message.Author.Mention} Sir, this is Wendy's #{model.WendysCounter}");
                //model.WendysCounter++;
                //_fileAccess.Write(model);
            }

            if (message.Content == "!rando")
            {
                var users = (await message.Channel.GetUsersAsync().FlattenAsync()).ToList();
                var user = users[_random.Next(users.Count())];

                await message.Channel.SendMessageAsync($"Hey {user.Mention}, I was told by {message.Author.Mention} that you have bad taste in knives and are not good at sharpening *OOOOHHHHH BUUURRRRNNNN*");
            }
        }
    }
}
