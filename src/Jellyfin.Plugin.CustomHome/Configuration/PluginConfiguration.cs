using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.CustomHome.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public string GreetingHeroTitleTemplate { get; set; } = "Goeiedag, {UserName}";

    public string[] GreetingHeroSubtitleOptions { get; set; } =
    {
        "Waar kijken we vandaag naar?",
        "Klaar voor een nieuwe filmavond?",
        "Pak er iets lekkers bij en druk op play.",
        "Tijd om iets nieuws te ontdekken."
    };

    public string? EditorsChoiceItemId { get; set; }
        = null; // Item Id (GUID string) van je persoonlijke aanbeveling

    public string EditorsChoiceTitle { get; set; } = "Editor's Choice";

    public bool EnableHeroSection { get; set; } = true;

    public bool EnableEditorsChoiceSection { get; set; } = true;
}
