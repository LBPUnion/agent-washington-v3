using Discord;
using Discord.WebSocket;

namespace LBPUnion.AgentWashington.Core;

public class CommandManager : BotModule
{
    private bool _hasBootstrapped = false;
    private bool _hasInitialized = false;
    private List<Command> _commands = new();

    public void RegisterCommand<T>() where T : Command, new()
    {
        var command = new T();

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new InvalidOperationException($"Command registered with an empty name!");

        if (_commands.Any(x => x.Name == command.Name))
            throw new InvalidOperationException("Two commands with the same name have been registered!");

        _commands.Add(command);

        if (_hasBootstrapped)
            command.Bootstrap(this.Modules);

        if (_hasInitialized)
            command.Initialize();
    }

    protected override void BeforeInit()
    {
        foreach (var command in _commands)
        {
            command.Bootstrap(Modules);
        }

        _hasBootstrapped = true;
    }

    protected override void Init()
    {
        foreach (var command in _commands)
            command.Initialize();
        _hasInitialized = true;
    }
    
    public async Task BuildSlashCommands(DiscordSocketClient client)
    {
        foreach (var command in _commands)
        {
            var builder = new SlashCommandBuilder();
            builder.WithName(command.Name);
            builder.WithDescription(command.Description);

            foreach (var option in command.Options)
            {
                builder.AddOption(option.Name, MapOptionType(option.Type), option.Description, option.IsRequired);
            }
            
            var slashCommand = builder.Build();

            foreach (var guild in client.Guilds)
            {
                var createdCommand =
                    await guild.CreateApplicationCommandAsync(slashCommand);
                
            }

        }
    }

    internal async Task ProcessSlashCommand(SocketSlashCommand commandData)
    {
        var command = _commands.FirstOrDefault(x => x.Name == commandData.CommandName);

        if (command == null)
            throw new InvalidOperationException($"Slash command {commandData.CommandName} not found.");

        await command.Handle(commandData);
    }

    internal static ApplicationCommandOptionType MapOptionType(OptionType type)
    {
        return type switch
        {
            OptionType.SubCommand => ApplicationCommandOptionType.SubCommand,
            OptionType.SubCommandGroup => ApplicationCommandOptionType.SubCommandGroup,
            OptionType.String => ApplicationCommandOptionType.String,
            OptionType.Integer => ApplicationCommandOptionType.Integer,
            OptionType.Boolean => ApplicationCommandOptionType.Boolean,
            OptionType.User => ApplicationCommandOptionType.User,
            OptionType.Channel => ApplicationCommandOptionType.Channel,
            OptionType.Role => ApplicationCommandOptionType.Role,
            OptionType.Mentionable => ApplicationCommandOptionType.Mentionable,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}