using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.TokenReplacers.Replacers
{
    public class NameTokenReplacer : BaseTokenReplacer, ITokenReplacer
    {
        public NameTokenReplacer(TokenReplacerContext context) : base(context)
        {
        }

        public override string Token
        {
            get
            {
                return "name";
            }
        }

        public void ReplaceTokens(ContentItemDisplay contentItem)
        {
            ReplaceTokens(contentItem, contentItem.Name);
        }
    }
}
