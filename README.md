# Agent Washington v3

Agent Washington is the friendly open-source LBP Union Discord bot. It provides useful features for the LittleBigPlanet community such as displaying the status of the LittleBigPlanet Online servers and [Project: Lighthouse](https://github.com/LBPUnion/project-lighthouse). Along with that, it supports custom plugins for extending the bot's functionality.

## Setup Guide

Assuming you know how to build a .NET solution, and that the binaries are stored in the hypothetical `bin` folder, follow these steps to get the bot running.

1. **Create a Discord bot.** Head over to the [Discord developer portal](https://discord.com/developers/applications), log in, and create a new application.
2. **Grant the necessary OAuth2 scopes**: You'll need the `bot` and `application:commands` scopes.
3. **Get a Bot Token**: Head to [Bot] and follow the instructions to generate a Bot Token. Copy it to your Clipboard for later.
4. **Invite the bot to your server.**
5. **Run the bot.** This will generate the default config (`config.json`) in the same folder as the bot's binaries. The bot will then exit, prompting you to give it your bot token. Do that.
6. **Run the bot again**. Now that the bot knows its token, everything should work. Check to see if there's a new `/hello` command and that it runs. If so, Agent Washington is working!

> **Note about OAuth2 Bot Permissions:** Because the bot can be extended with plugins, we cannot provide a concrete list of  what permissions the bot needs. This depends on what plugins you use. At the very least, make sure that  the bot can do anything a normal member of your server can.
>
> Each plugin should have permission information in its README.

## Configuring the Bot

Agent Washington's `config.json` contains the configuration for every plugin as well as the bot itself. Most plugins should let you configure them through commands, but if that's not the case, `config.json` is your next best bet.

One such important configuration is the "Developer ID." This is a Discord user that has **full, global access** to the bot. If you're hosting the bot, the Developer ID should be you. Use this to give yourself global admin access to finish configuring the rest of the bot.

Example: this will give m88youngling root access to the bot.

```json
{
    "PermissionSettings": {
        "Permissions": {
            "DeveloperUserId": 184071608921489408
        }
    }
}
```

### Permission Levels
As a form of security, Agent Washington has support for a basic permission system based on the user's role in your server. There are 4 permission levels.

 - **Default:** The default permission level for all users.
 - **Moderator:** Should be granted to roles with moderator privileges.
 - **Administrator:** Should be granted to server administrators.
 - **Developer:** Maintainer of the bot, has global administrator access. Can only be granted to one user, and can't be granted through bot commands.

For any given user, the role with the highest permission level takes precedence over the user's primary role. This means that, for example, if your server roles are laid out like so:

1. Admin
2. Moderator
3. Staff
4. Muted
5. Member
6. @everyone

...with `Admin` being assigned the "Moderator" permission and `Moderator" being assigned the "Administrator" permission (why would you do this?), and a user has both `Admin` and `Moderator` roles, the bot will see the user  as an admin even though their highest role has a lower permission level.

This also means that, if `Staff` is assigned the "Moderator" permission, then ANY user with the `Staff` role is seen as a moderator unless they have a role with higher permission level.

## Plugins

AWv3 plugins are created as C# class libraries. We have created a few base plugins, such as `LBPUnion.HttpMonitor`. If a plugin is built, you can place its DLL file in `bin/plugins` (where `bin` is the bot's binary location). The bot will load plugins at runtime.

### Building Plugins

To write your own plugin, follow these instructions:

1. **Clone this repository.** You'll need access to the `Bot` project to run the bot, and the `Core` project to use as the plugin SDK.
2. **Open the `src/LBPUnion.AgentWashington.sln` solution in your IDE of choice.
3. In the "Plugins" solution folder, create a new C# Class Library. Delete the `Class1.cs`.
4. Add a project reference to `LBPUnion.AgentWashington.Core`.
5. Edit the plugin's `.csproj` and add the following to the `<PropertyGroup>`. This will force the plugin to build straight into the bot plugins folder.

```xml
<PropertyGroup>
    <OutputPath>..\LBPUnion.AgentWashington.Bot\bin\$(Configuration)\$(TargetFramework)\plugins</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
</PropertyGroup>
```

6. Create a Build Dependency for the plugin inside of the solution. The process for doing this depends on your IDE, but it's a required step. Ensure that the `LBPUnion.AgentWashington.Bot` project has a build dependency on your plugin. Use `LBPUnion.HttpMonitor` as an example.
7. Create your plugin class! Use this code as a starting point.

```C#
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Logging;

namespace MyPlugin 
{
    [Plugin]
    public class MyPluginClass : BotModule 
    {
        private CommandManager _commands;
    
        protected override void BeforeInit()
        {
            // Use  BeforeInit() to do pre-initialization stuff and to request
            // access to other bot modules by using Modules.GetModule().
            Logger.Log("My plugin is initializing!");
            
            _commands = Modules.GetModule<CommandManager>();
        }
        
        protected override void Init()
        {
            // Use Init() to do initialization stuff that involves talking to other plugins.
            // Examples would be:
            //  - registering new Settings groups
            //  - registering slash commands with CommandManager.
        }
    }
}
```

8. Run the bot. You should see your plugin initializing by looking at the Console.