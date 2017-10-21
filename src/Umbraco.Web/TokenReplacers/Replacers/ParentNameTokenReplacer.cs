using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.TokenReplacers.Replacers
{
    public class ParentNameTokenReplacer : BaseTokenReplacer, ITokenReplacer
    {
        public ParentNameTokenReplacer(TokenReplacerContext context) : base(context)
        {
        }

        public override string Token
        {
            get
            {
                return "parentname";
            }
        }

        public void ReplaceTokens(ContentItemDisplay contentItem)
        {
            var parent = TokenReplacerContext.UmbracoContext.Application.Services.ContentService.GetById(contentItem.ParentId);
            ReplaceTokens(contentItem, parent.Name);
        }
    }
}
