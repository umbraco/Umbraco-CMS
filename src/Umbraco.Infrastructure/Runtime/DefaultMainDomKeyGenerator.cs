using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime;

internal class DefaultMainDomKeyGenerator : IMainDomKeyGenerator
{
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;

    public DefaultMainDomKeyGenerator(
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<GlobalSettings> globalSettings)
    {
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings;
    }

    public string GenerateKey()
    {
        var machineName = Environment.MachineName;
        var mainDomId = MainDom.GetMainDomId(_hostingEnvironment);
        var discriminator = _globalSettings.CurrentValue.MainDomKeyDiscriminator;

        var rawKey = $"{machineName}{mainDomId}{discriminator}";

        return rawKey.GenerateHash<SHA1>();
    }
}
