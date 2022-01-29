using Discord;
using Discord.WebSocket;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.AgentWashington.Core;

public class DiscordBot : BotModule
{
    private bool _ready;
    private bool _running = false;
    private SettingsManager _settings;
    private DiscordSocketClient _client;
    private ConnectionSettings _connectionSettings;
    private CommandManager _commands;
    
    private DiscordBot(ModuleManager moduleManager)
    {
        moduleManager.RegisterModule(this);
    }

    protected override void BeforeInit()
    {
        _client = new DiscordSocketClient();
        _settings = Modules.GetModule<SettingsManager>();
        _connectionSettings = _settings.RegisterSettings<ConnectionSettings>();

        _commands = Modules.GetModule<CommandManager>();
    }

    protected override void Init()
    {
        var token = _connectionSettings.Token;

        if (string.IsNullOrWhiteSpace(token))
        {
            Logger.Log(
                $"The ConnectionSettings/Token setting is blank. You must create a Discord Bot Application and fill the ConnectionSettings/Token setting with the bot's token. After doing this, restart the bot. The bot will exit.",
                LogLevel.Error);
            return;
        }

        Logger.Log("Attempting a connection to Discord...");
        _client.Log += HandleDiscordLoga;
        _client.Ready += HandleReady;
        _client.LoginAsync(TokenType.Bot, token).Wait();

        Logger.Log("Discord login success!", LogLevel.Message);
        
        _client.SlashCommandExecuted += HandleSlashCommand;
    }

    private Task HandleReady()
    {
        _ready = true;
        return Task.CompletedTask;
    }

    private Task HandleDiscordLoga(LogMessage arg)
    {
        var level = arg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Fatal,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Info,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Info
        };

        Logger.Log(arg.Message, level);
        if (arg.Exception != null)
        {
            Logger.Log(arg.Exception.ToString(), level);
        }

        return Task.CompletedTask;
    }

    private async Task HandleSlashCommand(SocketSlashCommand arg)
    {
        await arg.DeferAsync();

        try
        {
            await _commands.ProcessSlashCommand(arg);
        }
        catch (Exception ex)
        {
            Logger.Log("Unhandled exception in slash command.", LogLevel.Error);
            Logger.Log(ex.ToString(), LogLevel.Error);

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Red);
            embed.WithTitle("An error has occurred.");
            embed.WithDescription(
                "An error has occurred while processing that command. Please try again. If the problem persists, please contact a server administrator.");

            await arg.ModifyOriginalResponseAsync((props) => { props.Embed = embed.Build(); });
        }
    }

    private void Run()
    {
        if (_client.LoginState != LoginState.LoggedIn)
            return;
        
        Logger.Log("Bot is now running.");

        RunAsync().GetAwaiter().GetResult();
    }

    private async Task RunAsync()
    {
        Logger.Log("We've just kicked off into the land of asynchronous programming...");
        await _client.StartAsync();

        while (!_ready)
            await Task.Delay(10);
        
        await _commands.BuildSlashCommands(_client);
        
        _running = true;
        while (_running)
        {
            await Task.Delay(10);
        }
    }
    
    public static void Bootstrap()
    {
        Logger.Log("Starting bootstrap process...");
        var moduleManager = new ModuleManager();
        
        moduleManager.Bootstrap();

        var bot = new DiscordBot(moduleManager);
        bot.Run();
    }
}