using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Shocky.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(ShockyPlugin plugin) : base(
        "Config window",
        ImGuiWindowFlags.NoCollapse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(700, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Api info:");
        ImGui.Spacing();
        ImGui.Indent(30);
        var apiKey = configuration.ApiKey;
        if (ImGui.InputText("ApiKey", ref apiKey, 255))
        {
            configuration.ApiKey = apiKey;
        }
        var username = configuration.ShockUsername;
        if (ImGui.InputText("Username", ref username, 255))
        {
            configuration.ShockUsername = username;
        }
        ImGui.Spacing();
        var shockCode = configuration.ShockerCode;
        if (ImGui.InputText("Code", ref shockCode, 255))
        {
            configuration.ShockerCode = shockCode;
        }
        ImGui.Unindent(30);

        ImGui.BeginGroup();
        ImGui.Text("Listen to the triggers on:");
        ImGui.Indent(30);

        foreach (var chatEntry in (XivChatType[])Enum.GetValues(typeof(XivChatType)))
        {
            if (chatEntry == XivChatType.None || chatEntry == XivChatType.Debug || chatEntry == XivChatType.Urgent || chatEntry == XivChatType.Notice || chatEntry == XivChatType.TellOutgoing || chatEntry == XivChatType.Echo || chatEntry == XivChatType.ErrorMessage || chatEntry == XivChatType.GatheringSystemMessage || chatEntry == XivChatType.Notice
             || chatEntry == XivChatType.NoviceNetwork || chatEntry == XivChatType.CustomEmote || chatEntry == XivChatType.SystemMessage || chatEntry == XivChatType.SystemError || chatEntry == XivChatType.NPCDialogue || chatEntry == XivChatType.NPCDialogueAnnouncements || chatEntry == XivChatType.RetainerSale)
                continue;

            var configValue = configuration.ChatListeners.Contains(chatEntry);
            if (ImGui.Checkbox(chatEntry.ToString(), ref configValue))
            {
                if (configValue)
                {
                    configuration.ChatListeners.Add(chatEntry);
                }
                else
                {
                    configuration.ChatListeners.Remove(chatEntry);
                }
            }
        }

        foreach (var trigger in configuration.Triggers)
        {
            DrawTrigger(trigger);
        }

        if (ImGui.Button("Add Trigger"))
        {
            configuration.Triggers.Add(new Trigger());
        }

        if (ImGui.Button("Save"))
        {
            configuration.Save();
        }
    }

    private void DrawTrigger(Trigger trigger)
    {
        ImGui.BeginGroup();
        ImGui.Text("Trigger Word");
        ImGui.SameLine();
        var triggerWord = trigger.TriggerWord;
        if (ImGui.InputText($"##Word{trigger.Id}", ref triggerWord, 100))
        {
            trigger.TriggerWord = triggerWord;
        }

        ImGui.Text("Operation Type");
        ImGui.SameLine();
        var operationType = trigger.OperationType;
        if (ImGui.InputInt($"##OpType{trigger.Id}", ref operationType))
        {
            trigger.OperationType = operationType;
        }

        ImGui.Text("Duration");
        ImGui.SameLine();
        var duration = trigger.Duration;
        if (ImGui.InputInt($"##Dur{trigger.Id}", ref duration))
        {
            trigger.Duration = duration;
        }

        ImGui.Text("Intensity");
        ImGui.SameLine();
        var intensity = trigger.Intensity;
        if (ImGui.InputInt($"##Int{trigger.Id}", ref intensity))
        {
            trigger.Intensity = intensity;
        }

        if (ImGui.Button($"Remove##Rem{trigger.Id}"))
        {
            configuration.Triggers.Remove(trigger);
        }
        ImGui.EndGroup();

        ImGui.Separator();
    }
}
