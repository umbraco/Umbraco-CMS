using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

internal interface IDeliveryApiContentIndexHelper
{
    void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform);
}
