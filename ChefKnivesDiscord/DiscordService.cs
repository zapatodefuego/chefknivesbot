using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ChefKnivesDiscord
{
    public class DiscordService
    {
        private const long _modChannelId = 751633580748308490;
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public DiscordService(ILogger logger, IConfiguration configuration)
        {
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["DiscordToken"]);
            await _client.StartAsync();
        }

        public async Task SendModChannelMessage(string message)
        {
            var channel = _client.GetChannel(751633580748308490) as IMessageChannel;
            await channel.SendMessageAsync(message);
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

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("pong!");
        }
    }
}
