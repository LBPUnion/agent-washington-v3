using Discord;
using Discord.WebSocket;
using LBPUnion.AgentWashington.Core.Permissions;

namespace LBPUnion.AgentWashington.Core;

public abstract class Command
{
    private bool _isEphemeral = false;
    private Dictionary<string, object> _args = new();
    private string _textResponse;
    private Embed _responseEmbed = null;
    private ModuleManager _modules;
    private SocketSlashCommand _command;
    private SocketGuild _guild;
    
    public virtual string Name => GetType().Name;
    public virtual string Description => "There is no description for this command.";

    protected SocketUser User => _command.User;
    protected bool WasRunInGuild => _guild != null;
    protected SocketGuild Guild => _guild;

    protected ModuleManager Modules => _modules;

    protected virtual PermissionLevel MinimumPermissionLevel => PermissionLevel.Default;
    
    public virtual IEnumerable<Option> Options
    {
        get
        {
            yield break;
        }
    }

    internal void Bootstrap(ModuleManager modules)
    {
        _modules = modules;
        BeforeInit();
    }

    internal void Initialize()
    {
        Init();
    }

    private bool ReadOptions(SocketSlashCommand command)
    {
        _args.Clear();
        
        foreach (var option in this.Options)
        {
            var optionData = command.Data.Options.FirstOrDefault(x => x.Name == option.Name);

            if (optionData == null)
            {
                if (option.IsRequired)
                {
                    var builder = new EmbedBuilder();
                    builder.WithColor(Color.Red);
                    builder.WithTitle("Required parameter missing");
                    builder.WithDescription($"Expected {command.Type} parameter: {option.Name} - {option.Description}");

                    RespondWithEmbed(builder.Build());
                    return false;
                }

                continue;
            }

            var mappedType = CommandManager.MapOptionType(option.Type);
            if (optionData.Type != mappedType)
            {
                var builder = new EmbedBuilder();
                builder.WithColor(Color.Red);
                builder.WithTitle("Parameter type mismatch!");
                builder.WithDescription(
                    $"Expected {command.Type} parameter: {option.Name} - But you specified a parameter of type \"{optionData.Type}\" instead.");

                RespondWithEmbed(builder.Build());
                
                return false;
            }

            _args.Add(option.Name, optionData.Value);
        }

        return true;
    }

    protected void MakeEphemeral()
    {
        _isEphemeral = true;
    }

    private bool TestPermissions()
    {
        var required = (int) MinimumPermissionLevel;

        var permManager = Modules.GetModule<PermissionManager>();
        var userPerm = (int) permManager.GetPermissionLevel(User);

        var granted = userPerm >= required;

        if (!granted)
        {
            MakeEphemeral();
            var error = new EmbedBuilder();
            error.WithTitle("Access denied");
            error.WithDescription("You do not have permission to run this command.");
            error.WithColor(Color.Red);
            RespondWithEmbed(error.Build());
        }
        
        return granted;
    }
    
    internal async Task Handle(SocketSlashCommand command)
    {
        _isEphemeral = false;
        _command = command;

        if (_command.Channel is SocketTextChannel textChannel)
        {
            this._guild = textChannel.Guild;
        }
        
        var result = ReadOptions(command);

        result &= TestPermissions();

            if (result)
            await OnHandle();

        var embeds = new List<Embed>();
        var ephemeral = this._isEphemeral;
        if (_responseEmbed == null && string.IsNullOrWhiteSpace(_textResponse))
        {
            var successEmbed = new EmbedBuilder();
            successEmbed.WithTitle("Request fulfilled.");
            successEmbed.WithDescription("Your command has been fulfilled successfully.");
            successEmbed.WithColor(Color.Green);

            embeds.Add(successEmbed.Build());

            ephemeral = true;
        }
        else
        {
            if (_responseEmbed != null)
                embeds.Add(_responseEmbed);
        }

        await command.FollowupAsync(_textResponse, embeds.Any() ? embeds.ToArray() : null, false, ephemeral);
    }

    protected T GetArgument<T>(string argument)
    {
        if (TryGetArgument(argument, out T value))
            return value;
        return default;
    }

    protected bool TryGetArgument<T>(string argument, out T value)
    {
        if (_args.TryGetValue(argument, out var obj))
        {
            if (obj is T v)
            {
                value = v;
                return true;
            }
        }

        value = default;
        return false;
    }

    protected virtual void BeforeInit() {}
    protected virtual void Init() {}
    protected abstract Task OnHandle();

    protected void RespondWithText(string text)
    {
        _textResponse = text;
    }

    protected void RespondWithEmbed(Embed embed)
    {
        _responseEmbed = embed;
    }
}