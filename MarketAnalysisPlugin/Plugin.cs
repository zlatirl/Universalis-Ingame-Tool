using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MarketAnalysisPlugin.Windows;

namespace MarketAnalysisPlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Market Analysis Plugin";
        private const string CommandName = "/marketanalysis";

        private IDalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("MarketAnalysisPlugin");

        private MainWindow MainWindow { get; init; }
        private ConfigWindow ConfigWindow { get; init; }

        // Dalamud services
        public IDataManager DataManager { get; init; }
        public IClientState ClientState { get; init; }
        public ITextureProvider TextureProvider { get; init; }

        public Plugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IDataManager dataManager,
            IClientState clientState,
            ITextureProvider textureProvider)
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            DataManager = dataManager;
            ClientState = clientState;
            TextureProvider = textureProvider;

            // Load configuration
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            // Initialize windows
            MainWindow = new MainWindow(this);
            ConfigWindow = new ConfigWindow(this);

            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(ConfigWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the Market Analysis window."
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            // Register the main UI callback - THIS IS THE FIX
            PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            CommandManager.RemoveHandler(CommandName);

            // Remove callbacks
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            PluginInterface.UiBuilder.OpenMainUi -= DrawMainUI;
        }

        private void OnCommand(string command, string args)
        {
            // In case it's hidden
            DrawMainUI();
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        public void DrawMainUI()
        {
            MainWindow.IsOpen = true;
        }

        public void ToggleConfigUI()
        {
            ConfigWindow.IsOpen = !ConfigWindow.IsOpen;
        }
    }
}
