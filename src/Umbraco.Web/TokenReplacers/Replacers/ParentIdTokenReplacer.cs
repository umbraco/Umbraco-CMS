using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.TokenReplacers.Replacers
{
    public class ParentIdTokenReplacer : BaseTokenReplacer, ITokenReplacer
    {
        public ParentIdTokenReplacer(TokenReplacerContext context) : base(context)
        {
        }

        public override string Token
        {
            get
            {
                return "parentid";
            }
        }

        public void ReplaceTokens(ContentItemDisplay contentItem)
        {
            ReplaceTokens(contentItem, contentItem.ParentId.ToString());
        }
    }
}
