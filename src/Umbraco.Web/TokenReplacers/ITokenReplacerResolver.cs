using System.Collections.Generic;

namespace Umbraco.Web.TokenReplacers
{
    public interface ITokenReplacerResolver
    {
        IEnumerable<ITokenReplacer> TokenReplacers { get; }
    }
}