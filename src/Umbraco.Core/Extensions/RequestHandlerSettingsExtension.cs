using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Get concatenated user and default character replacements
    /// taking into account <see cref="RequestHandlerSettings.EnableDefaultCharReplacements"/>
    /// </summary>
    public static class RequestHandlerSettingsExtension
    {
        public static IEnumerable<CharItem> GetCharReplacements(this RequestHandlerSettings requestHandlerSettings)
        {
            if (!requestHandlerSettings.EnableDefaultCharReplacements)
            {
                return requestHandlerSettings.UserDefinedCharCollection ?? Enumerable.Empty<CharItem>();
            }

            if (requestHandlerSettings.UserDefinedCharCollection == null || !requestHandlerSettings.UserDefinedCharCollection.Any())
            {
                return RequestHandlerSettings.DefaultCharCollection;
            }

            foreach (CharItem defaultReplacement in RequestHandlerSettings.DefaultCharCollection)
            {
                foreach (CharItem userReplacement in requestHandlerSettings.UserDefinedCharCollection)
                {
                    if (userReplacement.Char == defaultReplacement.Char)
                    {
                        defaultReplacement.Replacement = userReplacement.Replacement;
                    }
                }
            }

            IEnumerable<CharItem> mergedCollections =
                RequestHandlerSettings.DefaultCharCollection.Union<CharItem>(
                    requestHandlerSettings.UserDefinedCharCollection, new CharacterReplacementEqualityComparer());

            return mergedCollections;
        }
    }
}
