using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace MarketAnalysisPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        // User preferences
        public int NotificationFrequency { get; set; } = 60; // Minutes
        public float PriceAlertThreshold { get; set; } = 20; // Percentage
        public List<string> SubscribedItems { get; set; } = new List<string>();
        public string SelectedWorld { get; internal set; }

        // The below exist just to make saving less cumbersome
        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
