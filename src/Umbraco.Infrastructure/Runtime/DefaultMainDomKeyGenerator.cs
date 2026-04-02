using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime;

internal sealed class DefaultMainDomKeyGenerator : IMainDomKeyGenerator
{
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultMainDomKeyGenerator"/> class.
    /// </summary>
    /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/> representing the current hosting environment.</param>
    /// <param name="globalSettings">The <see cref="IOptionsMonitor{GlobalSettings}"/> used to access global configuration settings.</param>
    public DefaultMainDomKeyGenerator(
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<GlobalSettings> globalSettings)
    {
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings;
    }

    /// <summary>
    /// Generates a unique, hashed key for MainDom based on the machine name, main domain ID, and a discriminator value.
    /// </summary>
    /// <returns>A SHA1-hashed string representing the generated key.</returns>
    public string GenerateKey()
    {
        var machineName = Environment.MachineName;
        var mainDomId = MainDom.GetMainDomId(_hostingEnvironment);
        var discriminator = _globalSettings.CurrentValue.MainDomKeyDiscriminator;

        var rawKey = $"{machineName}{mainDomId}{discriminator}";

        return rawKey.GenerateHash<SHA1>();
    }
}
