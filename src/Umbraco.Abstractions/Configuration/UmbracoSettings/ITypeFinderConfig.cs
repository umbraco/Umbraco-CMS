using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ITypeFinderConfig
    {
        IEnumerable<string> AssembliesAcceptingLoadExceptions { get; }
    }
}
