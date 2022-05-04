using Discord;
using Discord.WebSocket;
using LBPUnion.AgentWashington.Core;

namespace LBPUnion.ServerReminders.Commands;

public class AddGuildReminder : Command
{
    public override string Name => "add-reminder";
    public override string Description => "Add a recurring server reminder.";

    public override IEnumerable<Option> Options
    {
        get
        {
            yield return new Option("day-of-week", OptionType.Integer, "Day of week - value between 1 and 7", true);
            yield return new Option("hour", OptionType.Integer, "Hour of day, from 0-23, all times are UTC", true);
            yield return new Option("channel", OptionType.Integer, "Where should the reminder be posted?", true);
            yield return new Option("frequency", OptionType.Integer, "How many weeks between each reminder?", true);
            yield return new Option("message", OptionType.String, "What should be said?", true);
        }
    }

    protected override async Task OnHandle()
    {
        var text = GetArgument<string>("message");
        var weeks = GetArgument<long>("frequency");
        var channelId = GetArgument<long>("channel");
        var hour = GetArgument<long>("hour");
        var day = GetArgument<long>("day-of-week");
        var reminderManager = Modules.GetModule<ServerRemindersPlugin>();
        var guild = this.Guild;

        var embedBuilder = new EmbedBuilder();

        var channel = guild.GetTextChannel((ulong) channelId);

        if (channel == null)
        {
            embedBuilder
                .WithTitle("Cannot add reminder")
                .WithDescription("The specified text channel was not found on this server.")
                .WithColor(0xf71b1bu);

            RespondWithEmbed(embedBuilder.Build());
            return;
        }

        if (day < 1 || day > 7)
        {
            embedBuilder
                .WithTitle("Cannot add reminder")
                .WithDescription("The specified day of week must be a value between 1 and 7.")
                .WithColor(0xf71b1bu);

            RespondWithEmbed(embedBuilder.Build());
            return;
        }

        if (hour < 0 || hour > 23)
        {
            embedBuilder
                .WithTitle("Cannot add reminder")
                .WithDescription("The specified hour must be a value between 0 and 23.")
                .WithColor(0xf71b1bu);

            RespondWithEmbed(embedBuilder.Build());
            return;
        }

        var reminder = new ServerReminder
        {
            ChannelId = (ulong) channelId,
            GuildId = guild.Id,
            Text = text,
            DayOfWeek = (DayOfWeek) (day - 1),
            WeeksBetweenUpdate = (int) weeks,
            Hour = (int) hour
        };

        reminderManager.AddReminder(reminder);
        
        embedBuilder
            .WithTitle("Success!")
            .WithDescription("Server reminder has been created!")
            .WithColor(0x1baaf7u);

        RespondWithEmbed(embedBuilder.Build());
    }
}