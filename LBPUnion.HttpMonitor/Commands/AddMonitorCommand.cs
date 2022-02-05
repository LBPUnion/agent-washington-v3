using System.Globalization;
using System.Reflection.Metadata;
using Discord;
using LBPUnion.AgentWashington.Core;

namespace LBPUnion.HttpMonitor.Commands;

public class AddMonitorCommand : Command
{
    public override string Name => "add-server";
    public override string Description => "Add a new server to the Server Monitor list.";

    public override IEnumerable<Option> Options
    {
        get
        {
            yield return new Option("name", OptionType.String, "Server's friendly name", true);
            yield return new Option("host", OptionType.String, "Server hostname",
                true);
            yield return new Option("path", OptionType.String, "Request URL path",
                false);
            yield return new Option("port", OptionType.Integer, "Server port", false);
            yield return new Option("https", OptionType.Boolean,
                "Force use of HTTPS?", false);
            yield return new Option("trust-all-certs", OptionType.Boolean,
                "Ignore invalid/expired SSL certificates?",
                false);
        }
    }

    protected override Task OnHandle()
    {
        var name = GetArgument<string>("name");
        var host = GetArgument<string>("host");

        if (!TryGetArgument<string>("path", out var path))
            path = "/";

        if (!TryGetArgument<int>("port", out var port))
            port = 80;

        if (!TryGetArgument<bool>("https", out var https))
            https = false;

        if (https == true && port == 80)
            port = 443;

        if (!TryGetArgument<bool>("trust-all-certs", out var trustAll))
            trustAll = false;

        var builder = new EmbedBuilder();
        builder.WithColor(Color.Green);
        builder.WithTitle("Added new server: " + name);
        builder.WithDescription("The bot is now monitoring the server specified below.");

        builder.AddField("Host", $"{host}:{port}");
        builder.AddField("Request Path", path);
        builder.AddField("Use HTTPS", https);
        builder.AddField("Trust All Certificates", trustAll);

        RespondWithEmbed(builder.Build());
        
        return Task.CompletedTask;
    }
}