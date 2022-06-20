using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core;

public static class UdiParserServiceConnectors
{
    private static readonly object ScanLocker = new();

    // notes - see U4-10409
    // if this class is used during application pre-start it cannot scans the assemblies,
    // this is addressed by lazily-scanning, with the following caveats:
    // - parsing a root udi still requires a scan and therefore still breaks
    // - parsing an invalid udi ("umb://should-be-guid/<not-a-guid>") corrupts KnowUdiTypes
    private static volatile bool _scanned;

    /// <summary>
    ///     Scan for deploy <see cref="IServiceConnector" /> in assemblies for known UDI types.
    /// </summary>
    /// <param name="typeLoader"></param>
    public static void ScanDeployServiceConnectorsForUdiTypes(TypeLoader typeLoader)
    {
        if (typeLoader is null)
        {
            throw new ArgumentNullException(nameof(typeLoader));
        }

        if (_scanned)
        {
            return;
        }

        lock (ScanLocker)
        {
            // Scan for unknown UDI types
            // there is no way we can get the "registered" service connectors, as registration
            // happens in Deploy, not in Core, and the Udi class belongs to Core - therefore, we
            // just pick every service connectors - just making sure that not two of them
            // would register the same entity type, with different udi types (would not make
            // much sense anyways)
            IEnumerable<Type> connectors = typeLoader.GetTypes<IServiceConnector>();
            var result = new Dictionary<string, UdiType>();
            foreach (Type connector in connectors)
            {
                IEnumerable<UdiDefinitionAttribute>
                    attrs = connector.GetCustomAttributes<UdiDefinitionAttribute>(false);
                foreach (UdiDefinitionAttribute attr in attrs)
                {
                    if (result.TryGetValue(attr.EntityType, out UdiType udiType) && udiType != attr.UdiType)
                    {
                        throw new Exception(string.Format(
                            "Entity type \"{0}\" is declared by more than one IServiceConnector, with different UdiTypes.",
                            attr.EntityType));
                    }

                    result[attr.EntityType] = attr.UdiType;
                }
            }

            // merge these into the known list
            foreach (KeyValuePair<string, UdiType> item in result)
            {
                UdiParser.RegisterUdiType(item.Key, item.Value);
            }

            _scanned = true;
        }
    }

    /// <summary>
    ///     Registers a single <see cref="IServiceConnector" /> to add it's UDI type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RegisterServiceConnector<T>()
        where T : IServiceConnector
    {
        var result = new Dictionary<string, UdiType>();
        Type connector = typeof(T);
        IEnumerable<UdiDefinitionAttribute> attrs = connector.GetCustomAttributes<UdiDefinitionAttribute>(false);
        foreach (UdiDefinitionAttribute attr in attrs)
        {
            if (result.TryGetValue(attr.EntityType, out UdiType udiType) && udiType != attr.UdiType)
            {
                throw new Exception(string.Format(
                    "Entity type \"{0}\" is declared by more than one IServiceConnector, with different UdiTypes.",
                    attr.EntityType));
            }

            result[attr.EntityType] = attr.UdiType;
        }

        // merge these into the known list
        foreach (KeyValuePair<string, UdiType> item in result)
        {
            UdiParser.RegisterUdiType(item.Key, item.Value);
        }
    }
}
