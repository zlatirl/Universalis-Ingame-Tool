> 
> This is a GitHub Template repo. If you want to make your own plugin, go to [this template](https://github.com/goatcorp/SamplePlugin) and click "Use this template" to make a new repo!
>

# Market Analysis Plugin

Simple Market Analysis plugin for Dalamud.

A Marketboard analysis tool for Final Fantasy XIV, helping players track prices and optimise their buying and selling strategies.

## Features

* Functional Plugin
  * Slash command
  * Main UI
  * Settings UI
  * Plugin json
* Real-time Market Data: Connects to Universalis WebSocket for up-to-date Market Information
* Price Tracking: View current listings and price history for items

## Installation

### Prerequisites

Market Analysis Plugin assumes all the following prerequisites are met:

* XIVLauncher, FINAL FANTASY XIV, and Dalamud have all been installed and the game has been run with Dalamud at least once.
* XIVLauncher is installed to its default directories and configurations.
* FINAL FANTASY XIV with an active subscription

### Building

1. Open up `MarketAnalysisPlugin.sln` in your C# editor of choice (likely [Visual Studio 2022](https://visualstudio.microsoft.com) or [JetBrains Rider](https://www.jetbrains.com/rider/)).
2. Build the solution. By default, this will build a `Debug` build, but you can switch to `Release` in your IDE.
3. The resulting plugin can be found at `Universalis-Ingame-Tool/bin/x64/Debug/MarketAnalysisPlugin.dll` (or `Release` if appropriate.)

### Activating in-game

1. Launch the game and use `/xlsettings` in chat or `xlsettings` in the Dalamud Console to open up the Dalamud settings.
    * In here, go to `Experimental`, and add the full path to the `MarketAnalysisPlugin.dll` to the list of Dev Plugin Locations.
2. Next, use `/xlplugins` (chat) or `xlplugins` (console) to open up the Plugin Installer.
    * In here, go to `Dev Tools > Installed Dev Plugins`, and the `Market Analysis Plugin` should be visible. Enable it.
3. You should now be able to use `/marketanalysis` (chat)!

Note that you only need to add it to the Dev Plugin Locations once (Step 1); it is preserved afterwards. You can disable, enable, or load your plugin on startup through the Plugin Installer.

### Development

This plugin is built on the Dalamud Plugin framework. To either contribute or make your own plugin:

For this plugin:
  1. Clone the repository
  2. Setup your development environment with .NET Core 8 SDK
  3. Make changes as you wish - then build the solution
  4. Follow steps from Activating in-game to test your changes in-game

To make your own plugin:
  1. Go to [SamplePlugin](https://github.com/goatcorp/SamplePlugin) repository and follow the steps there.

  For information about Dalamud plugin development, see the [Dalamud Developer Docs](https://dalamud.dev/).

## Acknowledgements

* Market data provided by [Universalis](https://universalis.app/)
* Built using the Dalamud plugin framework
* Special thanks to the FFXIV community & my friends who play FFXIV for feedback and testing