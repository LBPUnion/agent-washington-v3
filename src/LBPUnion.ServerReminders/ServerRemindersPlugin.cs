using Discord.WebSocket;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Persistence;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.ServerReminders.Commands;

namespace LBPUnion.ServerReminders;

[Plugin]
public class ServerRemindersPlugin : BotModule
{
    private DatabaseManager database;
    private SettingsManager settingsManager;
    private CommandManager commandManager;
    private readonly List<ServerReminder> reminders = new List<ServerReminder>();

    protected override void Init()
    {
        RestoreState();

        commandManager.RegisterCommand<AddGuildReminder>();
    }

    protected override void BeforeInit()
    {
        Logger.Log("ServerReminders plugin is initializing");
        settingsManager = this.Modules.GetModule<SettingsManager>();
        database = this.Modules.GetModule<DatabaseManager>();
        commandManager = this.Modules.GetModule<CommandManager>();

        base.BeforeInit();
    }

    protected override void OnTick(UpdateInterval interval)
    {
        var currentDate = DateTime.UtcNow;

        foreach (var reminder in reminders)
        {
            if (reminder.DayOfWeek == currentDate.DayOfWeek && reminder.Hour == currentDate.Hour)
            {
                var hasHitMaxWeeks = reminder.WeeksSinceLastUpdate >= reminder.WeeksBetweenUpdate;
                if (hasHitMaxWeeks)
                    reminder.WeeksSinceLastUpdate = 0;
                else
                    reminder.WeeksSinceLastUpdate++;

                if (hasHitMaxWeeks)
                    PostReminder(reminder);

                SaveState();
            }
        }   
    }

    public void AddReminder(ServerReminder reminder)
    {
        this.reminders.Add(reminder);
        SaveState();
    }
    
    private void RestoreState()
    {
        Logger.Log("Restoring server reminder state from database...");

        database.OpenDatabase((db) =>
        {
            reminders.Clear();
            var dbReminders = db.GetCollection<ServerReminder>(nameof(ServerReminder));
            foreach (var reminder in dbReminders.FindAll())
            {
                Logger.Log($"{reminder.Id} loaded.");
                reminders.Add(reminder);
            }
        });
    }

    private void PostReminder(ServerReminder reminder)
    {
        var bot = this.Modules.GetModule<DiscordBot>();
        var guilds = bot.GetGuilds();
        var guild = guilds.FirstOrDefault(x => x.Id == reminder.GuildId);
        if (guild == null)
            return;

        var textChannel = guild.GetChannel(reminder.ChannelId) as SocketTextChannel;
        if (textChannel == null)
            return;

        try
        {
            textChannel.SendMessageAsync(reminder.Text).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Log(
                $"Couldn't post server reminder {reminder.Id} to guild {reminder.GuildId}, channel {reminder.ChannelId}: An exception was thrown by Discord's API. {ex.Message}",
                LogLevel.Warning);
        }
    }
    
    private void SaveState()
    {
        this.database.OpenDatabase(SaveStateInternal);
    }

    private void SaveStateInternal(Database db)
    {
        var reminderCollection = db.GetCollection<ServerReminder>(nameof(ServerReminder));

        Logger.Log("Saving server reminder state to Database...");

        foreach (var reminder in this.reminders)
        {
            if (reminderCollection.FindOne(x => x.Id == reminder.Id) != null)
            {
                Logger.Log($"{reminder.Id} updated in database");
                reminderCollection.Update(reminder);
            }
            else
            {
                Logger.Log($"{reminder.Id} saved to database");
                reminderCollection.Insert(reminder);
            }
        }

        var deleted = reminderCollection.Find(x => this.reminders.All(y => y.Id != x.Id)).ToArray();

        foreach (var deletedReminder in deleted)
        {
            Logger.Log($"{deletedReminder.Id} removed from database");
            reminderCollection.Delete(deletedReminder.Id);
        }
    }
}

public class ServerReminder : IDatabaseObject
{
    public int WeeksSinceLastUpdate { get; set; }
    public int WeeksBetweenUpdate { get; set; }
    public int Hour { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public string Text { get; set; }
    public Guid Id { get; set; }
}