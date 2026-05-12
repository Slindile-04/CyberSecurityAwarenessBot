using System.Text.Json;

namespace CyberSecurityAwarenessBot.Services
{
    /// <summary>
    /// ThemeService.cs - Manages application themes and visual styling
    /// 
    /// Features:
    /// - Load and apply cybersecurity-themed colors
    /// - Support for multiple theme profiles (Dark, Light, HighContrast)
    /// - Save/load user theme preferences
    /// - Provide consistent color palette across the application
    /// </summary>
    public class ThemeService
    {
        private readonly string _themeConfigPath;
        private Dictionary<string, ThemeConfiguration> _themes;

        public ThemeService(string configPath = "Resources/Themes")
        {
            _themeConfigPath = configPath;
            _themes = new Dictionary<string, ThemeConfiguration>();
            InitializeDefaultThemes();
        }

        /// <summary>
        /// Initializes default cybersecurity-themed color schemes
        /// </summary>
        private void InitializeDefaultThemes()
        {
            // Dark theme (primary)
            _themes["Dark"] = new ThemeConfiguration
            {
                Name = "Dark",
                BackgroundColor = "#14141e",
                ForegroundColor = "#c8c8c8",
                AccentColor = "#00ff96",
                WarningColor = "#ff6464",
                ErrorColor = "#ff3232"
            };

            // Light theme
            _themes["Light"] = new ThemeConfiguration
            {
                Name = "Light",
                BackgroundColor = "#ffffff",
                ForegroundColor = "#333333",
                AccentColor = "#0066cc",
                WarningColor = "#ff6600",
                ErrorColor = "#cc0000"
            };

            // High contrast for accessibility
            _themes["HighContrast"] = new ThemeConfiguration
            {
                Name = "HighContrast",
                BackgroundColor = "#000000",
                ForegroundColor = "#ffff00",
                AccentColor = "#00ff00",
                WarningColor = "#ff0000",
                ErrorColor = "#ff00ff"
            };
        }

        /// <summary>
        /// Gets a theme by name
        /// </summary>
        public ThemeConfiguration GetTheme(string themeName)
        {
            if (_themes.TryGetValue(themeName, out var theme))
                return theme;

            return _themes["Dark"]; // Default to Dark theme
        }

        /// <summary>
        /// Gets all available themes
        /// </summary>
        public List<string> GetAvailableThemes()
        {
            return _themes.Keys.ToList();
        }

        /// <summary>
        /// Saves theme preference to configuration
        /// </summary>
        public void SaveThemePreference(string themeName)
        {
            try
            {
                string configFile = Path.Combine(_themeConfigPath, "theme-preference.json");
                Directory.CreateDirectory(_themeConfigPath);

                var config = new { SelectedTheme = themeName };
                string json = JsonSerializer.Serialize(config);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme preference: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// ThemeConfiguration - Holds color and styling information for a theme
    /// </summary>
    public class ThemeConfiguration
    {
        public string Name { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
        public string AccentColor { get; set; }
        public string WarningColor { get; set; }
        public string ErrorColor { get; set; }
    }
}
