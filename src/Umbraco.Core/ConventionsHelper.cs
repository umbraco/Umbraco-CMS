using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core;

public static class ConventionsHelper
{
    public static Dictionary<string, PropertyType> GetStandardPropertyTypeStubs(IShortStringHelper shortStringHelper) =>
        new()
        {
            {
                Constants.Conventions.Member.Comments,
                new PropertyType(
                    shortStringHelper,
                    Constants.PropertyEditors.Aliases.TextArea,
                    ValueStorageType.Ntext,
                    true,
                    Constants.Conventions.Member.Comments)
                { Name = Constants.Conventions.Member.CommentsLabel }
            },
        };
}
