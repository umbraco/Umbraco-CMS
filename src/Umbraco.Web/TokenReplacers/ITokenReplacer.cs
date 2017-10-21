using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.TokenReplacers
{
    public interface ITokenReplacer
    {
        void ReplaceTokens(ContentItemDisplay contentItem);
    }
}
