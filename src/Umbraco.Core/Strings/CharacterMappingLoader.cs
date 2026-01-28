using System.Collections.Frozen;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Loads character mappings from embedded JSON files and user configuration.
/// </summary>
public sealed class CharacterMappingLoader : ICharacterMappingLoader
{
    private static readonly string[] BuiltInFiles =
        ["ligatures.json", "special-latin.json", "cyrillic.json", "extended-mappings.json"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<CharacterMappingLoader> _logger;

    public CharacterMappingLoader(
        IHostEnvironment hostEnvironment,
        ILogger<CharacterMappingLoader> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <inheritdoc />
    public FrozenDictionary<char, string> LoadMappings()
    {
        var allMappings = new List<(int Priority, string Name, Dictionary<string, string> Mappings)>();

        // 1. Load built-in mappings from embedded resources
        foreach (var file in BuiltInFiles)
        {
            var mapping = LoadEmbeddedMapping(file);
            if (mapping != null)
            {
                allMappings.Add((mapping.Priority, mapping.Name, mapping.Mappings));
                _logger.LogDebug(
                    "Loaded built-in character mappings: {Name} ({Count} entries)",
                    mapping.Name, mapping.Mappings.Count);
            }
        }

        // 2. Load user mappings from config directory
        var userPath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "config",
            "character-mappings");

        if (Directory.Exists(userPath))
        {
            foreach (var file in Directory.GetFiles(userPath, "*.json"))
            {
                var mapping = LoadJsonFile(file);
                if (mapping != null)
                {
                    allMappings.Add((mapping.Priority, mapping.Name, mapping.Mappings));
                    _logger.LogInformation(
                        "Loaded user character mappings: {Name} ({Count} entries, priority {Priority})",
                        mapping.Name, mapping.Mappings.Count, mapping.Priority);
                }
            }
        }

        // 3. Merge by priority (higher priority wins)
        return MergeMappings(allMappings);
    }

    private FrozenDictionary<char, string> MergeMappings(
        List<(int Priority, string Name, Dictionary<string, string> Mappings)> allMappings)
    {
        var merged = new Dictionary<char, string>();

        foreach (var (_, name, mappings) in allMappings.OrderBy(m => m.Priority))
        {
            foreach (var (key, value) in mappings)
            {
                if (key.Length == 1)
                {
                    merged[key[0]] = value;
                }
                else if (key.Length > 1)
                {
                    // Multi-character keys are not supported for single-character mapping
                    // This could happen if someone adds multi-character keys in their custom mapping files
                    _logger.LogWarning(
                        "Skipping multi-character key '{Key}' in mapping '{Name}' - only single characters are supported",
                        key, name);
                }
            }
        }

        return merged.ToFrozenDictionary();
    }

    private CharacterMappingFile? LoadEmbeddedMapping(string fileName)
    {
        var assembly = typeof(CharacterMappingLoader).Assembly;
        var resourceName = $"Umbraco.Cms.Core.Strings.CharacterMappings.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            _logger.LogWarning(
                "Built-in character mapping file not found: {ResourceName}",
                resourceName);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CharacterMappingFile>(stream, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse embedded mapping: {ResourceName}", resourceName);
            return null;
        }
    }

    private CharacterMappingFile? LoadJsonFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var mapping = JsonSerializer.Deserialize<CharacterMappingFile>(json, JsonOptions);

            if (mapping?.Mappings == null)
            {
                _logger.LogWarning(
                    "Invalid mapping file {Path}: missing 'mappings' property", path);
                return null;
            }

            return mapping;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse character mappings from {Path}", path);
            return null;
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to read character mappings from {Path}", path);
            return null;
        }
    }
}
