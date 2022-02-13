using System.Globalization;
using System.Reflection.Metadata;
using Discord;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Permissions;
using LBPUnion.HttpMonitor.Settings;

namespace LBPUnion.HttpMonitor.Commands;

public class AddMonitorCommand : Command
{
    public override string Name => "add-server";
    public override string Description => "Add a new server to the Server Monitor list.";

    protected override PermissionLevel MinimumPermissionLevel => PermissionLevel.Administrator;

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
        var monitor = Modules.GetModule<MonitorPlugin>();
        var name = GetArgument<string>("name");
        var host = GetArgument<string>("host");

        if (!TryGetArgument<string>("path", out var path))
            path = "/";

        if (!TryGetArgument<long>("port", out var port))
            port = 80;

        if (!TryGetArgument<bool>("https", out var https))
            https = false;

        if (https == true && port == 80)
            port = 443;

        if (!TryGetArgument<bool>("trust-all-certs", out var trustAll))
            trustAll = false;

        if (monitor.TargetExists(Guild, name))
        {
            var error = new EmbedBuilder();
            error.WithColor(Color.Red);
            error.WithTitle("Cannot add server: " + name);
            error.WithDescription("A server with this name already exists in the monitor list.");
            
            RespondWithEmbed(error.Build());

            return Task.CompletedTask;
        }
        
        var target = new MonitorTarget
        {
            Name = name,
            Host = host,
            Port = (ushort) port,
            Path = path,
            UseSsl = https,
            IgnoreSslCertErrors = trustAll
        };
        monitor.AddTarget(this.Guild, target);
        
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