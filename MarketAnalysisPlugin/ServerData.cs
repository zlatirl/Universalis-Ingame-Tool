using System;
using System.Collections.Generic;

namespace MarketAnalysisPlugin.Data;

// Class containing data about FFXIV servers, data centers, and regions
public static class ServerData
{
    // Dictionary mapping data centers to their respective worlds
    public static Dictionary<string, List<uint>> DataCenters = new Dictionary<string, List<uint>>
    {
        // Europe
        { "Europe", new List<uint> {
            // Chaos DC
            80, 83, 71, 39, 401, 97, 400, 85,
            // Light DC
            402, 36, 66, 56, 403, 67, 33, 42
        }},
        // North America
        { "North-America", new List<uint> {
            // Aether DC
            73, 79, 54, 63, 40, 65, 99, 57,
            // Crystal DC
            91, 34, 74, 62, 81, 75, 37, 41,
            // Dynamis DC
            408, 411, 406, 409, 407, 404, 410, 405,
            // Primal DC
            78, 93, 53, 35, 95, 55, 64, 77
        }},
        // Oceania
        { "Oceania", new List<uint> {
            // Materia DC
            22, 21, 86, 87, 88
        }},
        // Japan
        { "Japan", new List<uint> {
            // Elemental DC
            90, 68, 45, 58, 94, 49, 72, 50,
            // Gaia DC
            43, 69, 92, 46, 59, 98, 76, 51,
            // Mana DC
            44, 23, 70, 47, 48, 96, 28, 61,
            // Meteor DC
            24, 82, 60, 29, 30, 52, 31, 32
        }}
    };

    // Nested Dictionary that groups Data Centers by region
    public static Dictionary<string, Dictionary<string, List<uint>>> DataCenterGroups = new Dictionary<string, Dictionary<string, List<uint>>>
    {
        { "Europe", new Dictionary<string, List<uint>> {
            { "Light", new List<uint> { 402, 36, 66, 56, 403, 67, 33, 42 } },
            { "Chaos", new List<uint> { 80, 83, 71, 39, 401, 97, 400, 85 } }
        }},
        { "North-America", new Dictionary<string, List<uint>> {
            { "Aether", new List<uint> { 73, 79, 54, 63, 40, 65, 99, 57 } },
            { "Crystal", new List<uint> { 91, 34, 74, 62, 81, 75, 37, 41 } },
            { "Primal", new List<uint> { 78, 93, 53, 35, 95, 55, 64, 77 } },
            { "Dynamis", new List<uint> { 408, 411, 406, 409, 407, 404, 410, 405 } }
        }},
        { "Japan", new Dictionary<string, List<uint>> {
            { "Elemental", new List<uint> { 90, 68, 45, 58, 94, 49, 72, 50 } },
            { "Gaia", new List<uint> { 43, 69, 92, 46, 59, 98, 76, 51 } },
            { "Mana", new List<uint> { 44, 23, 70, 47, 48, 96, 28, 61 } },
            { "Meteor", new List<uint> { 24, 82, 60, 29, 30, 52, 31, 32 } }
        }},
        { "Oceania", new Dictionary<string, List<uint>> {
            { "Materia", new List<uint> { 22, 21, 86, 87, 88 } }
        }}
    };

    // Dictionary mapping world IDs to world names
    public static Dictionary<uint, string> Worlds = new Dictionary<uint, string>
    {
        // European Worlds
        { 80, "Cerberus" }, { 83, "Louisoix" }, { 71, "Moogle" }, { 39, "Omega" },
        { 401, "Phantom" }, { 97, "Ragnarok" }, { 400, "Sagittarius" }, { 85, "Spriggan" },
        { 402, "Alpha" }, { 36, "Lich" }, { 66, "Odin" }, { 56, "Phoenix" },
        { 403, "Raiden" }, { 67, "Shiva" }, { 33, "Twintania" }, { 42, "Zodiark" },

        // American Worlds
        { 73, "Adamantoise" }, { 79, "Cactuar" }, { 54, "Faerie" }, { 63, "Gilgamesh" },
        { 40, "Jenova" }, { 65, "Midgardsormr" }, { 99, "Sargatanas" }, { 57, "Siren" },
        { 91, "Balmung" }, { 34, "Brynhildr" }, { 74, "Coeurl" }, { 62, "Diabolos" },
        { 81, "Goblin" }, { 75, "Malboro" }, { 37, "Mateus" }, { 41, "Zalera" },
        { 408, "Cuchulainn" }, { 411, "Golem" }, { 406, "Halicarnassus" }, { 409, "Kraken" },
        { 407, "Maduin" }, { 404, "Marilith" }, { 410, "Rafflesia" }, { 405, "Seraph" },
        { 78, "Behemoth" }, { 93, "Excalibur" }, { 53, "Exodus" }, { 35, "Famfrit" },
        { 95, "Hyperion" }, { 55, "Lamia" }, { 64, "Leviathan" }, { 77, "Ultros" },

        // Oceanic Worlds
        { 22, "Bismarck" }, { 21, "Ravana" }, { 86, "Sephirot" }, { 87, "Sophia" }, { 88, "Zurvan" },

        // Japanese Worlds
        { 90, "Aegis" }, { 68, "Atomos" }, { 45, "Carbuncle" }, { 58, "Garuda" },
        { 94, "Gungnir" }, { 49, "Kujata" }, { 72, "Tonberry" }, { 50, "Typhon" },
        { 43, "Alexander" }, { 69, "Bahamut" }, { 92, "Durandal" }, { 46, "Fenrir" },
        { 59, "Ifrit" }, { 98, "Ridill" }, { 76, "Tiamat" }, { 51, "Ultima" },
        { 44, "Anima" }, { 23, "Asura" }, { 70, "Chocobo" }, { 47, "Hades" },
        { 48, "Ixion" }, { 96, "Masamune" }, { 28, "Pandaemonium" }, { 61, "Titan" },
        { 24, "Belias" }, { 82, "Mandragora" }, { 60, "Ramuh" }, { 29, "Shinryu" },
        { 30, "Unicorn" }, { 52, "Valefor" }, { 31, "Yojimbo" }, { 32, "Zeromus" }
    };

    // Helper method to get World ID from name
    public static uint GetWorldIdByName(string worldName)
    {
        foreach (var pair in Worlds)
        {
            if (pair.Value == worldName)
                return pair.Key;
        }
        return 0; // Return 0 if not found
    }

    // Helper method to get Data Center name from World ID
    public static string GetDataCenterFromWorldId(uint worldId)
    {
        foreach (var dcGroup in DataCenterGroups)
        {
            foreach (var dc in dcGroup.Value)
            {
                if (dc.Value.Contains(worldId))
                    return dc.Key;
            }
        }
        return string.Empty;
    }

    // Helper method to get Region name from World ID
    public static string GetRegionFromWorldId(uint worldId)
    {
        foreach (var region in DataCenterGroups)
        {
            foreach (var dc in region.Value)
            {
                if (dc.Value.Contains(worldId))
                    return region.Key;
            }
        }
        return string.Empty;
    }
}
