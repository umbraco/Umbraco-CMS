using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.Examine.ValueSetBuilders;

internal interface IDeliveryApiContentIndexHelper
{
    void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform);
}
