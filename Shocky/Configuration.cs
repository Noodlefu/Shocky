using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace Shocky
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public string ApiKey { get; set; } = string.Empty;
        public string ShockerCode { get; set; } = string.Empty;
        public string ShockUsername { get; set; } = string.Empty;

        public readonly List<Trigger> Triggers = [];

        public List<XivChatType> ChatListeners { get; set; } = [];

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface!.SavePluginConfig(this);
        }
    }
}
