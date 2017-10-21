using System;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.TokenReplacers.Replacers
{
    public class DateTimeTokenReplacer : BaseTokenReplacer, ITokenReplacer
    {
        public DateTimeTokenReplacer(TokenReplacerContext context) : base(context)
        {
        }

        public override string Token
        {
            get
            {
                return "datetime";
            }
        }

        public void ReplaceTokens(ContentItemDisplay contentItem)
        {
            ReplaceTokens(contentItem, DateTime.Now);
        }
    }
}
