using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Text.Json;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Dalamud.Utility;

namespace MarketAnalysisPlugin.Windows;

// Displays market data, listings, and analysis for FFXIV items.
public class MainWindow : Window, IDisposable
{
    // Reference to main plugin instance
    private Plugin Plugin;

    // Search-related fields
    private string searchText = string.Empty;
    private List<Item> filteredItems = new List<Item>();

    // Market data storage
    private Dictionary<uint, MarketListing[]> currentListings = new Dictionary<uint, MarketListing[]>();
    private Dictionary<uint, MarketListing[]> historyListings = new Dictionary<uint, MarketListing[]>();

    // For selected item display
    private Item? selectedItem = null;
    private uint selectedItemId = 0;
    private bool isLoadingMarketData = false;
    private string marketDataError = string.Empty;
    private string selectedItemName = string.Empty;

    // Category navigation
    private Dictionary<uint, string> categoryNames = new Dictionary<uint, string>();
    private Dictionary<uint, List<uint>> categoryItems = new Dictionary<uint, List<uint>>();
    private int expandedCategory = -1;
    private bool isShowingSearchResults = false;

    // Server selection
    private string selectedWorld = string.Empty;
    private string lastUpdateTime = string.Empty;
    private string lastFetchTime = string.Empty;

    // Tabs for the item detail view
    private int currentTab = 0;
    private readonly string[] tabs = { "Market Data", "Charts" };

    // Universalis client for WebSocket connection
    private UniversalisClient universalisClient;
    private bool isConnected = false;
    private string connectionStatus = "Not connected";

    // Sets up the window, initialises the Universalis WebSocket client, and loads item categories.
    public MainWindow(Plugin plugin)
        : base("Market Board", ImGuiWindowFlags.MenuBar)
    {
        // Set window constraints
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;

        // Initialise Universalis WebSocket client for real-time market data
        universalisClient = new UniversalisClient();

        // Set up event handlers for connection status changes
        universalisClient.OnConnected += (sender, args) => {
            isConnected = true;
            connectionStatus = "Connected to Universalis";
        };

        universalisClient.OnDisconnected += (sender, args) => {
            isConnected = false;
            connectionStatus = "Disconnected from Universalis";
        };

        // Set up handler for received market data from WebSocket
        universalisClient.OnMarketDataReceived += (sender, args) => {
            ProcessMarketData(args.RawMessage);
        };

        // Load item categories
        LoadItemCategories();
    }

    // Populates categoryNames and categoryItems dictionaries.
    private void LoadItemCategories()
    {
        try
        {
            var sheet = Plugin.DataManager.GetExcelSheet<ItemUICategory>();
            if (sheet == null) return;

            // Load category names
            foreach (var category in sheet)
            {
                if (category.RowId > 0 && !string.IsNullOrEmpty(category.Name.ToString())) // Convert ReadOnlyString to string
                {
                    categoryNames[category.RowId] = category.Name.ToString(); // Convert ReadOnlyString to string
                    categoryItems[category.RowId] = new List<uint>();
                }
            }

            // Assign items to categories
            var itemSheet = Plugin.DataManager.GetExcelSheet<Item>();
            if (itemSheet == null) return;

            foreach (var item in itemSheet)
            {
                // Only add items that are searchable and belong to a valid category
                if (item.ItemSearchCategory.RowId > 0 && categoryItems.ContainsKey(item.ItemUICategory.RowId))
                {
                    categoryItems[item.ItemUICategory.RowId].Add(item.RowId);
                }
            }
        }
        catch (Exception ex)
        {
            
        }
    }
    public void Dispose()
    {
        universalisClient?.Dispose();
    }

    public override void Draw()
    {
        DrawMenuBar();

        // Main window layout - sidebar and content area
        if (ImGui.BeginTable("##MainLayout", 2, ImGuiTableFlags.None))
        {
            ImGui.TableSetupColumn("Sidebar", ImGuiTableColumnFlags.WidthFixed, 250);
            ImGui.TableSetupColumn("Content", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();

            // Left sidebar with categories and search
            ImGui.TableNextColumn();
            DrawSidebar();

            // Main content area - item details view
            ImGui.TableNextColumn();
            DrawContentArea();

            ImGui.EndTable();
        }

        // Status bar at the bottom
        DrawStatusBar();
    }

    private void DrawMenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Exit"))
                {
                    IsOpen = false; // Close window
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("About"))
                {
                    // Show about dialog
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    private void DrawSidebar()
    {
        // Search bar with real-time updates
        ImGui.SetNextItemWidth(-1);

        // Store the previous search text to detect changes
        string previousSearchText = searchText;

        // Use standard input text without EnterReturnsTrue flag
        ImGui.InputTextWithHint("##SearchItems", "Search for item", ref searchText, 100);

        // Check if search text changed
        if (searchText != previousSearchText)
        {
            // Only search if there's at least 2 characters (to avoid searching on every keystroke)
            if (searchText.Length >= 2)
            {
                SearchItems();
                isShowingSearchResults = true;
            }
            else if (string.IsNullOrWhiteSpace(searchText))
            {
                // Clear search results if search box is empty
                isShowingSearchResults = false;
                filteredItems.Clear();
            }
        }

        // Advanced search button
        Vector4 advSearchColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
        ImGui.TextColored(advSearchColor, "Advanced Search");
        ImGui.SameLine();
        if (ImGui.Button("🔍"))
        {
            SearchItems();
            isShowingSearchResults = !string.IsNullOrWhiteSpace(searchText);
        }

        // Show search results or categories
        if (ImGui.BeginChild("##Categories", new Vector2(-1, -1), true))
        {
            if (isShowingSearchResults)
            {
                // Display search results
                if (filteredItems.Count > 0)
                {
                    ImGui.Text($"Search Results: {filteredItems.Count} items found");
                    ImGui.Separator();

                    foreach (var item in filteredItems)
                    {
                        ImGui.PushID((int)item.RowId);
                        if (ImGui.Selectable(item.Name.ToString(), selectedItemId == item.RowId))
                        {
                            selectedItem = item;
                            selectedItemId = item.RowId;
                            selectedItemName = item.Name.ToString();
                            FetchMarketDataForItem(item.RowId);
                        }
                        ImGui.PopID();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(searchText))
                {
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "No items found matching your search.");
                }

                // Add button to clear search results
                if (ImGui.Button("Clear Search"))
                {
                    searchText = string.Empty;
                    isShowingSearchResults = false;
                    filteredItems.Clear();
                }
            }
            else
            {
                // Display categories (your existing category code)
                var sortedCategories = categoryNames.OrderBy(c => c.Value).ToList();

                foreach (var category in sortedCategories)
                {
                    // Skip empty categories
                    if (!categoryItems.ContainsKey(category.Key) || categoryItems[category.Key].Count == 0)
                        continue;

                    bool isExpanded = expandedCategory == category.Key;

                    // Category header with arrow indicator
                    ImGui.PushID((int)category.Key);
                    if (ImGui.Selectable($"▶ {category.Value}", isExpanded))
                    {
                        expandedCategory = isExpanded ? -1 : (int)category.Key;
                    }
                    ImGui.PopID();

                    // Show items if category is expanded
                    if (expandedCategory == category.Key)
                    {
                        ImGui.Indent(20);

                        var itemSheet = Plugin.DataManager.GetExcelSheet<Item>();
                        if (itemSheet != null)
                        {
                            foreach (var itemId in categoryItems[category.Key])
                            {
                                var item = itemSheet.GetRow(itemId);
                                if (item.RowId == 0) continue;

                                ImGui.PushID((int)itemId);
                                if (ImGui.Selectable(item.Name.ToString(), selectedItemId == itemId))
                                {
                                    selectedItem = item;
                                    selectedItemId = itemId;
                                    selectedItemName = item.Name.ToString();
                                    FetchMarketDataForItem(itemId);
                                }
                                ImGui.PopID();
                            }
                        }

                        ImGui.Unindent(20);
                    }
                }
            }

            ImGui.EndChild();
        }
    }

    private void DrawContentArea()
    {
        if (selectedItem == null)
        {
            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Select an item from the sidebar or search for an item.");
            return;
        }

        // Item header with icon and name
        DrawItemHeader();

        // Tabs for different views
        if (ImGui.BeginTabBar("##ItemTabs"))
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                if (ImGui.BeginTabItem(tabs[i]))
                {
                    currentTab = i;
                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        // Content based on selected tab
        if (currentTab == 0) // Market Data
        {
            DrawMarketDataTab();
        }
        else if (currentTab == 1) // Charts
        {
            DrawChartsTab();
        }
    }

    private void DrawItemHeader()
    {
        ImGui.BeginGroup();
        ImGui.Dummy(new Vector2(40, 40)); // Placeholder for icon
        ImGui.EndGroup();

        ImGui.SameLine();

        // Item name in large font
        ImGui.BeginGroup();
        ImGui.PushFont(ImGui.GetFont());
        ImGui.Text(selectedItemName);
        ImGui.PopFont();
        ImGui.EndGroup();

        // Server info on the right
        ImGui.SameLine(ImGui.GetWindowWidth() - 200);
        ImGui.BeginGroup();
        ImGui.Text("Zodiark ↑");
        ImGui.Text($"Last update: {lastUpdateTime}");
        ImGui.Text($"Last Fetch: {lastFetchTime}");
        ImGui.EndGroup();
    }

    private void DrawMarketDataTab()
    {
        if (isLoadingMarketData)
        {
            ImGui.Text("Loading market data...");
            return;
        }

        if (!string.IsNullOrEmpty(marketDataError))
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), $"Error: {marketDataError}");
            if (ImGui.Button("Retry"))
            {
                FetchMarketDataForItem(selectedItemId);
            }
            return;
        }

        // Current listings section
        ImGui.Text("Current listings (Includes 5% GST)");

        if (currentListings.TryGetValue(selectedItemId, out var listings) && listings.Length > 0)
        {
            DrawListingsTable(listings, false);
        }
        else
        {
            ImGui.Text("No current listings available.");
        }

        ImGui.Separator();

        // Recent history section
        ImGui.Text("Recent history");

        if (historyListings.TryGetValue(selectedItemId, out var history) && history.Length > 0)
        {
            DrawHistoryTable(history);
        }
        else
        {
            ImGui.Text("No sale history available.");
        }
    }

    private void DrawListingsTable(MarketListing[] listings, bool isHistory)
    {
        try
        {
            // Create table with scrolling support
            if (ImGui.BeginTable("##ListingsTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY,
                new Vector2(-1, ImGui.GetContentRegionAvail().Y * 0.4f)))
            {
                // Table headers
                ImGui.TableSetupColumn("HQ", ImGuiTableColumnFlags.WidthFixed, 40);
                ImGui.TableSetupColumn("Price", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Qty", ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableSetupColumn("Total", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Retainer", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Table rows
                foreach (var listing in listings)
                {
                    ImGui.TableNextRow();

                    // HQ indicator
                    ImGui.TableNextColumn();
                    if (listing.IsHQ)
                    {
                        ImGui.Text("★"); // HQ symbol
                    }

                    // Price
                    ImGui.TableNextColumn();
                    ImGui.Text($"₹{listing.PricePerUnit:N0}");

                    // Quantity
                    ImGui.TableNextColumn();
                    ImGui.Text($"{listing.Quantity}");

                    // Total
                    ImGui.TableNextColumn();
                    ImGui.Text($"₹{listing.TotalPrice:N0}");

                    // Retainer
                    ImGui.TableNextColumn();
                    ImGui.Text($"{listing.RetainerName} ★ Zodiark");
                }

                ImGui.EndTable(); // Make sure we always end the table
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void DrawHistoryTable(MarketListing[] history)
    {
        try
        {
            // Create table with scrolling support
            if (ImGui.BeginTable("##HistoryTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY,
                new Vector2(-1, ImGui.GetContentRegionAvail().Y * 0.6f)))
            {
                // Table headers
                ImGui.TableSetupColumn("HQ", ImGuiTableColumnFlags.WidthFixed, 40);
                ImGui.TableSetupColumn("Price", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Qty", ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableSetupColumn("Total", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Date", ImGuiTableColumnFlags.WidthFixed, 150);
                ImGui.TableSetupColumn("Buyer", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Table rows
                foreach (var sale in history)
                {
                    ImGui.TableNextRow();

                    // HQ indicator
                    ImGui.TableNextColumn();
                    if (sale.IsHQ)
                    {
                        ImGui.Text("★"); // HQ symbol
                    }

                    // Price
                    ImGui.TableNextColumn();
                    ImGui.Text($"₹{sale.PricePerUnit:N0}");

                    // Quantity
                    ImGui.TableNextColumn();
                    ImGui.Text($"{sale.Quantity}");

                    // Total
                    ImGui.TableNextColumn();
                    ImGui.Text($"₹{sale.TotalPrice:N0}");

                    // Date
                    ImGui.TableNextColumn();
                    ImGui.Text(sale.LastReviewTime.ToString("dd/MM/yyyy HH:mm:ss"));

                    // Buyer
                    ImGui.TableNextColumn();
                    ImGui.Text($"{sale.RetainerName} ★ Zodiark");
                }

                ImGui.EndTable(); // Make sure we always end the table
            }
        }
        catch (Exception ex)
        {
        
        }
    }

    private void DrawChartsTab()
    {
        ImGui.Text("Price and quantity history charts will be displayed here.");

        // Placeholder for chart area
        ImGui.BeginChild("##ChartPlaceholder", new Vector2(-1, 300), true);
        ImGui.EndChild();
    }

    private void DrawStatusBar()
    {
        ImGui.Separator();
        ImGui.BeginGroup();

        // Left side - connection status
        ImGui.Text($"Status: {connectionStatus}");

        // Right side - credits
        ImGui.SameLine(ImGui.GetWindowWidth() - 200);
        ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), "Data provided by Universalis");

        ImGui.EndGroup();
    }

    // Search items in the game's item database
    private void SearchItems()
    {
        filteredItems.Clear();

        if (string.IsNullOrWhiteSpace(searchText))
            return;

        var itemSheet = Plugin.DataManager.GetExcelSheet<Item>();
        if (itemSheet == null)
            return;

        // Convert search text to lowercase for case-insensitive comparison
        string searchLower = searchText.ToLower();

        foreach (var item in itemSheet)
        {
            // Skip items that can't be traded on the market
            if (item.ItemSearchCategory.RowId == 0)
                continue;

            // Convert item name to string and check if it contains the search text
            string itemName = item.Name.ToString().ToLower();
            if (itemName.Contains(searchLower))
            {
                filteredItems.Add(item);

                // Open search results section
                if (expandedCategory != -99)
                {
                    expandedCategory = -99; // Special value for search results
                }

                // Limit search results to avoid performance issues
                if (filteredItems.Count >= 100)
                    break;
            }
        }
    }

    // Fetch market data for an item from Universalis
    private async void FetchMarketDataForItem(uint itemId)
    {
        isLoadingMarketData = true;
        marketDataError = string.Empty;

        try
        {
            // Determine the server or data center to query
            string server = string.IsNullOrEmpty(selectedWorld) ? "Zodiark" : selectedWorld;

            // Construct the URL for the Universalis API
            string url = $"https://universalis.app/api/v2/{server}/{itemId}?listings=30&entries=20";

            // Fetch data using HttpClient
            using (var client = new System.Net.Http.HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                ProcessMarketData(responseBody);

                // Update timestamps
                DateTime now = DateTime.Now;
                lastUpdateTime = now.ToString("dd/MM/yyyy HH:mm:ss tt");
                lastFetchTime = now.ToString("dd/MM/yyyy HH:mm:ss tt");
            }
        }
        catch (Exception ex)
        {
            marketDataError = ex.Message;
        }
        finally
        {
            isLoadingMarketData = false;
        }
    }

    // Process market data received from Universalis
    private void ProcessMarketData(string jsonData)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var marketData = JsonSerializer.Deserialize<UniversalisResponse>(jsonData, options);

            if (marketData == null) return;

            uint itemId = marketData.ItemID;

            // Process current listings
            if (marketData.Listings != null && marketData.Listings.Count > 0)
            {
                var listings = new List<MarketListing>();

                foreach (var listing in marketData.Listings)
                {
                    listings.Add(new MarketListing
                    {
                        ItemId = (int)itemId,
                        PricePerUnit = listing.PricePerUnit,
                        Quantity = listing.Quantity,
                        WorldName = listing.WorldName,
                        RetainerName = listing.RetainerName,
                        IsHQ = listing.Hq,
                        LastReviewTime = DateTimeOffset.FromUnixTimeSeconds(listing.LastReviewTime).DateTime
                    });
                }

                currentListings[itemId] = listings.ToArray();
            }

            // Process sales history
            if (marketData.RecentHistory != null && marketData.RecentHistory.Count > 0)
            {
                var history = new List<MarketListing>();

                foreach (var sale in marketData.RecentHistory)
                {
                    history.Add(new MarketListing
                    {
                        ItemId = (int)itemId,
                        PricePerUnit = sale.PricePerUnit,
                        Quantity = sale.Quantity,
                        WorldName = sale.WorldName,
                        RetainerName = sale.BuyerName, // Store buyer name in RetainerName field
                        IsHQ = sale.Hq,
                        LastReviewTime = DateTimeOffset.FromUnixTimeSeconds(sale.Timestamp).DateTime
                    });
                }

                historyListings[itemId] = history.ToArray();
            }
        }
        catch (Exception ex)
        {

        }
    }
}

// Helper classes for JSON deserialisation
public class UniversalisResponse
{
    public uint ItemID { get; set; }
    public List<UniversalisListing> Listings { get; set; }
    public List<UniversalisHistoryEntry> RecentHistory { get; set; }
    public Dictionary<string, long> WorldUploadTimes { get; set; }
}

public class UniversalisListing
{
    public int PricePerUnit { get; set; }
    public int Quantity { get; set; }
    public string WorldName { get; set; }
    public string RetainerName { get; set; }
    public bool Hq { get; set; }
    public long LastReviewTime { get; set; }
}

public class UniversalisHistoryEntry
{
    public int PricePerUnit { get; set; }
    public int Quantity { get; set; }
    public string WorldName { get; set; }
    public string BuyerName { get; set; }
    public bool Hq { get; set; }
    public long Timestamp { get; set; }
}
