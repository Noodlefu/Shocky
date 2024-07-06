global using Dalamud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XivCommon;

// From: https://github.com/UnknownX7

namespace Dalamud;

public class DalamudApi
{
    [PluginService]
    public static IDalamudPluginInterface? PluginInterface { get; private set; }

    [PluginService]
    public static IChatGui? ChatGui { get; private set; }

    [PluginService]
    public static IClientState? ClientState { get; private set; }

    [PluginService]
    public static ICommandManager? CommandManager { get; private set; }

    [PluginService]
    public static ICondition? Condition { get; private set; }

    [PluginService]
    public static IFramework? Framework { get; private set; }

    [PluginService]
    public static IGameInteropProvider? GameInteropProvider { get; private set; }

    [PluginService]
    public static IGameNetwork? GameNetwork { get; private set; }

    [PluginService]
    public static INotificationManager? NotificationManager { get; private set; }

    [PluginService]
    public static IPluginLog? PluginLog { get; private set; }

    [PluginService]
    public static ISigScanner? SigScanner { get; private set; }

    [PluginService]
    public static IToastGui? ToastGui { get; private set; }

    public static XivCommonBase? XivCommonBase { get; private set; }

    private static PluginCommandManager<IDalamudPlugin>? PluginCommandManager;
    private const string PrintName = "Blackjack";
    private const string PrintHeader = $"[{PrintName}] ";

    public DalamudApi() { }

    public DalamudApi(IDalamudPlugin plugin) => PluginCommandManager ??= new(plugin);

    public DalamudApi(IDalamudPlugin plugin, IDalamudPluginInterface pluginInterface)
    {
        if (!pluginInterface.Inject(this))
        {
            LogError("Failed loading DalamudApi!");
            return;
        }

        PluginCommandManager ??= new(plugin);
    }

    public static DalamudApi operator +(DalamudApi container, object o)
    {
        foreach (var f in typeof(DalamudApi).GetProperties())
        {
            if (f.PropertyType != o.GetType()) continue;
            if (f.GetValue(container) != null) break;
            f.SetValue(container, o);
            return container;
        }
        throw new InvalidOperationException();
    }

    public static void PrintEcho(string message) => ChatGui?.Print($"{PrintHeader}{message}");

    public static void PrintError(string message) => ChatGui?.PrintError($"{PrintHeader}{message}");

    public static void ShowNotification(string message, NotificationType type = NotificationType.None, uint msDelay = 3_000u) => NotificationManager?.AddNotification(new Notification { Type = type, Title = PrintName, Content = message, InitialDuration = TimeSpan.FromMilliseconds(msDelay) });

    public static void ShowToast(string message, ToastOptions? options = null) => ToastGui?.ShowNormal($"{PrintHeader}{message}", options);

    public static void ShowQuestToast(string message, QuestToastOptions? options = null) => ToastGui?.ShowQuest($"{PrintHeader}{message}", options);

    public static void ShowErrorToast(string message) => ToastGui?.ShowError($"{PrintHeader}{message}");

    public static void LogVerbose(string message, Exception? exception = null) => PluginLog?.Verbose(exception, message);

    public static void LogDebug(string message, Exception? exception = null) => PluginLog?.Debug(exception, message);

    public static void LogInfo(string message, Exception? exception = null) => PluginLog?.Information(exception, message);

    public static void LogWarning(string message, Exception? exception = null) => PluginLog?.Warning(exception, message);

    public static void LogError(string message, Exception? exception = null) => PluginLog?.Error(exception, message);

    public static void LogFatal(string message, Exception? exception = null) => PluginLog?.Fatal(exception, message);

    public static void Initialize(IDalamudPlugin plugin, IDalamudPluginInterface pluginInterface) => _ = new DalamudApi(plugin, pluginInterface);

    public static void Dispose() => PluginCommandManager?.Dispose();
}

#region PluginCommandManager
public class PluginCommandManager<T> : IDisposable where T : IDalamudPlugin
{
    private readonly T plugin;
    private readonly (string, CommandInfo)[] pluginCommands;

    public PluginCommandManager(T p)
    {
        plugin = p;
        pluginCommands = plugin.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
            .SelectMany(GetCommandInfoTuple)
            .ToArray();

        AddCommandHandlers();
    }

    private void AddCommandHandlers()
    {
        foreach (var (command, commandInfo) in pluginCommands)
            DalamudApi.CommandManager?.AddHandler(command, commandInfo);
    }

    private void RemoveCommandHandlers()
    {
        foreach (var (command, _) in pluginCommands)
            DalamudApi.CommandManager?.RemoveHandler(command);
    }

    private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(MethodInfo method)
    {
        var handlerDelegate = (IReadOnlyCommandInfo.HandlerDelegate)Delegate.CreateDelegate(typeof(IReadOnlyCommandInfo.HandlerDelegate), plugin, method);

        var command = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>();
        var aliases = handlerDelegate.Method.GetCustomAttribute<AliasesAttribute>();
        var helpMessage = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();
        var doNotShowInHelp = handlerDelegate.Method.GetCustomAttribute<DoNotShowInHelpAttribute>();

        var commandInfo = new CommandInfo(handlerDelegate)
        {
            HelpMessage = helpMessage?.HelpMessage ?? string.Empty,
            ShowInHelp = doNotShowInHelp == null,
        };

        // Create list of tuples that will be filled with one tuple per alias, in addition to the base command tuple.
        var commandInfoTuples = new List<(string, CommandInfo)> { (command!.Command, commandInfo) };
        if (aliases != null)
            commandInfoTuples.AddRange(aliases.Aliases.Select(alias => (alias, commandInfo)));

        return commandInfoTuples;
    }

    public void Dispose()
    {
        RemoveCommandHandlers();
        GC.SuppressFinalize(this);
    }
}
#endregion

#region Attributes
[AttributeUsage(AttributeTargets.Method)]
public class AliasesAttribute(params string[] aliases) : Attribute
{
    public string[] Aliases { get; } = aliases;
}

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}

[AttributeUsage(AttributeTargets.Method)]
public class DoNotShowInHelpAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class HelpMessageAttribute(string helpMessage) : Attribute
{
    public string HelpMessage { get; } = helpMessage;
}
#endregion
