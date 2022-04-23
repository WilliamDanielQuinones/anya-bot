namespace App;

public interface IBot
{
    Task Run(IServiceProvider services);
}

public class Bot : IBot
{
    private readonly IConfiguration _config;

    private DiscordSocketClient _client;
    private CommandService _commands;
    private DiscordConfig Config;
    
    public Bot(IConfiguration config)
    {
        _config = config;
        Config = _config.GetSection("Discord").Get<DiscordConfig>();
    }

    public async Task Run(IServiceProvider services)
    {
        var token = Config.Token;
        if (token == null) throw new Exception("Discord Token not set");
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,

            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            MessageCacheSize = 50
        });

        _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Info,
            CaseSensitiveCommands = false,
        });

        _client.Log += Log;
        _commands.Log += Log;

        await InitCommands(services);
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        

        await Task.Delay(-1);
    }

    private async Task HandleCommandAsync(SocketMessage arg, IServiceProvider services)
    {
        // Bail out if it's a System Message.
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

        // Create a number to track where the prefix ends and the command begins
        int pos = 0;
        // Uncomment the second half if you also want commands to be invoked by mentioning the bot instead.
        if (msg.HasCharPrefix('d', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
        {
            Console.WriteLine(msg);
            // Create a Command Context.
            var context = new SocketCommandContext(_client, msg);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully).
            var result = await _commands.ExecuteAsync(context, pos, services);
            // Uncomment the following lines if you want the bot
            // to send a message if it failed.
            // This does not catch errors from commands with 'RunMode.Async',
            // subscribe a handler for '_commands.CommandExecuted' to see those.
            //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            //    await msg.Channel.SendMessageAsync(result.ErrorReason);
        }
    }

    private static Task Log(LogMessage message)
    {
        switch (message.Severity)
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
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();

        return Task.CompletedTask;
    }

    private async Task InitCommands(IServiceProvider services)
    {
        // Module classes MUST be marked 'public' or they will be ignored.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        // Subscribe a handler to see if a message invokes a command.
        _client.MessageReceived += arg => HandleCommandAsync(arg, services);
    }
}

public class DiscordConfig
{
    public string? Token { get; set; }
}