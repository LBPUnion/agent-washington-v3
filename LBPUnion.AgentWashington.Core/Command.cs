using Discord;
using Discord.WebSocket;

namespace LBPUnion.AgentWashington.Core;

public abstract class Command
{
    private string _textResponse;
    private Embed _responseEmbed = null;
    private ModuleManager _modules;
    private SocketSlashCommand _command;
    
    public virtual string Name => GetType().Name;
    public virtual string Description => "There is no description for this command.";

    internal void Bootstrap(ModuleManager modules)
    {
        _modules = modules;
        BeforeInit();
    }

    internal void Initialize()
    {
        Init();
    }

    internal async Task Handle(SocketSlashCommand command)
    {
        _command = command;
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