using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

internal interface IDeliveryApiContentIndexHelper
{
    void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform);
}
