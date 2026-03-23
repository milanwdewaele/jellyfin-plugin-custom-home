using System.Reflection;
using System.Runtime.Loader;
using Jellyfin.Plugin.CustomHome.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.CustomHome;

public class CustomHomeEntryPoint : IServerEntryPoint
{
    private readonly ILogger<CustomHomeEntryPoint> _logger;
    private readonly IUserManager _userManager;
    private readonly ILibraryManager _libraryManager;
    private readonly IServiceProvider _serviceProvider;

    public CustomHomeEntryPoint(
        ILogger<CustomHomeEntryPoint> logger,
        IUserManager userManager,
        ILibraryManager libraryManager,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _userManager = userManager;
        _libraryManager = libraryManager;
        _serviceProvider = serviceProvider;
    }

    public Task RunAsync()
    {
        _logger.LogInformation("CustomHomeEntryPoint starting, attempting to register Home Screen Sections...");

        TryRegisterWithHomeScreenSections();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    private void TryRegisterWithHomeScreenSections()
    {
        var homeScreenSectionsAssembly = AssemblyLoadContext.All
            .SelectMany(x => x.Assemblies)
            .FirstOrDefault(x => x.FullName?.Contains(".HomeScreenSections") ?? false);

        if (homeScreenSectionsAssembly == null)
        {
            _logger.LogWarning("Home Screen Sections assembly not found; Custom Home sections will not be registered.");
            return;
        }

        var pluginInterfaceType = homeScreenSectionsAssembly.GetType("Jellyfin.Plugin.HomeScreenSections.PluginInterface");
        if (pluginInterfaceType == null)
        {
            _logger.LogWarning("PluginInterface type not found in Home Screen Sections assembly.");
            return;
        }

        var method = pluginInterfaceType.GetMethod("RegisterSection", BindingFlags.Public | BindingFlags.Static);
        if (method == null)
        {
            _logger.LogWarning("RegisterSection method not found on PluginInterface.");
            return;
        }

        RegisterHeroSection(method);
        RegisterEditorsChoiceSection(method);
    }

    private void RegisterHeroSection(MethodInfo registerMethod)
    {
        if (!CustomHomePlugin.Instance.Configuration.EnableHeroSection)
        {
            return;
        }

        var payload = new JObject
        {
            ["id"] = "8a9e9d0a-0a6a-4e7d-9d5f-5a4b2a0e1001",
            ["displayText"] = "Custom Hero",
            ["limit"] = 1,
            ["route"] = "/web/index.html#!/home.html",
            ["additionalData"] = "hero",
            ["resultsAssembly"] = GetType().Assembly.FullName,
            ["resultsClass"] = typeof(CustomHomeEntryPoint).FullName,
            ["resultsMethod"] = nameof(GetHeroItems)
        };

        registerMethod.Invoke(null, new object?[] { payload });
        _logger.LogInformation("Registered Custom Home hero section with Home Screen Sections.");
    }

    private void RegisterEditorsChoiceSection(MethodInfo registerMethod)
    {
        if (!CustomHomePlugin.Instance.Configuration.EnableEditorsChoiceSection)
        {
            return;
        }

        var payload = new JObject
        {
            ["id"] = "8a9e9d0a-0a6a-4e7d-9d5f-5a4b2a0e1002",
            ["displayText"] = CustomHomePlugin.Instance.Configuration.EditorsChoiceTitle,
            ["limit"] = 1,
            ["route"] = "/web/index.html#!/home.html",
            ["additionalData"] = "editorschoice",
            ["resultsAssembly"] = GetType().Assembly.FullName,
            ["resultsClass"] = typeof(CustomHomeEntryPoint).FullName,
            ["resultsMethod"] = nameof(GetEditorsChoiceItems)
        };

        registerMethod.Invoke(null, new object?[] { payload });
        _logger.LogInformation("Registered Custom Home editor's choice section with Home Screen Sections.");
    }

    public QueryResult<BaseItemDto> GetHeroItems(dynamic payload)
    {
        // Voor nu leveren we geen echt item maar een lege lijst.
        // De Media Bar plugin gebruikt zijn eigen logica voor de hero.
        return new QueryResult<BaseItemDto>
        {
            Items = Array.Empty<BaseItemDto>(),
            TotalRecordCount = 0
        };
    }

    public QueryResult<BaseItemDto> GetEditorsChoiceItems(dynamic payload)
    {
        var config = CustomHomePlugin.Instance.Configuration;
        if (string.IsNullOrWhiteSpace(config.EditorsChoiceItemId))
        {
            return new QueryResult<BaseItemDto>
            {
                Items = Array.Empty<BaseItemDto>(),
                TotalRecordCount = 0
            };
        }

        if (!Guid.TryParse(config.EditorsChoiceItemId, out var itemId))
        {
            return new QueryResult<BaseItemDto>
            {
                Items = Array.Empty<BaseItemDto>(),
                TotalRecordCount = 0
            };
        }

        var item = _libraryManager.GetItemById(itemId) as BaseItem;
        if (item == null)
        {
            return new QueryResult<BaseItemDto>
            {
                Items = Array.Empty<BaseItemDto>(),
                TotalRecordCount = 0
            };
        }

        var dto = new BaseItemDto
        {
            Name = item.Name,
            Id = item.Id,
            Type = item.GetType().Name,
            Overview = item.Overview,
            ImageTags = new Dictionary<ImageType, string>(),
            RunTimeTicks = item.RunTimeTicks
        };

        return new QueryResult<BaseItemDto>
        {
            Items = new[] { dto },
            TotalRecordCount = 1
        };
    }
}
