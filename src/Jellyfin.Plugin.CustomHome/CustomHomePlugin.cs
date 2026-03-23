using Jellyfin.Plugin.CustomHome.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.CustomHome;

public class CustomHomePlugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public CustomHomePlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "Custom Home";

    public override string Description => "Adds a configurable greeting hero and editor's choice section to the home screen via Home Screen Sections.";

    public static CustomHomePlugin Instance { get; private set; } = null!;

    public override Guid Id => new("c3cf7f0e-5bb0-4de4-9689-4c8f6e2fd111");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "customhomeconfig",
                EmbeddedResourcePath = string.Format(
                    "{0}.Web.customhomeconfig.html",
                    GetType().Namespace)
            }
        };
    }
}
