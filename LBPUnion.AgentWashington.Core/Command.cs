using Discord;
using Discord.WebSocket;

namespace LBPUnion.AgentWashington.Core;

public abstract class Command
{
    private Dictionary<string, object> _args = new();
    private string _textResponse;
    private Embed _responseEmbed = null;
    private ModuleManager _modules;
    private SocketSlashCommand _command;
    
    public virtual string Name => GetType().Name;
    public virtual string Description => "There is no description for this command.";

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
    
    internal async Task Handle(SocketSlashCommand command)
    {
        _command = command;

        var result = ReadOptions(command);
        
        if (result)
            await OnHandle();

        await command.ModifyOriginalResponseAsync((response) =>
        {
            response.Content = _textResponse;
            response.Embed = _responseEmbed;

            if (string.IsNullOrWhiteSpace(this._textResponse) && this._responseEmbed == null)
            {
                response.Content = "The command was successful.";
            }
        });
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