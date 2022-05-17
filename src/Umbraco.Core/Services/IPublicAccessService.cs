using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IPublicAccessService : IService
{
    /// <summary>
    ///     Gets all defined entries and associated rules
    /// </summary>
    /// <returns></returns>
    IEnumerable<PublicAccessEntry> GetAll();

    /// <summary>
    ///     Gets the entry defined for the content item's path
    /// </summary>
    /// <param name="content"></param>
    /// <returns>Returns null if no entry is found</returns>
    PublicAccessEntry? GetEntryForContent(IContent content);

    /// <summary>
    ///     Gets the entry defined for the content item based on a content path
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns>Returns null if no entry is found</returns>
    PublicAccessEntry? GetEntryForContent(string contentPath);

    /// <summary>
    ///     Returns true if the content has an entry for it's path
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Attempt<PublicAccessEntry?> IsProtected(IContent content);

    /// <summary>
    ///     Returns true if the content has an entry based on a content path
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns></returns>
    Attempt<PublicAccessEntry?> IsProtected(string contentPath);

    /// <summary>
    ///     Adds a rule if the entry doesn't already exist
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ruleType"></param>
    /// <param name="ruleValue"></param>
    /// <returns></returns>
    Attempt<OperationResult<OperationResultType, PublicAccessEntry>?> AddRule(IContent content, string ruleType, string ruleValue);

    /// <summary>
    ///     Removes a rule
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ruleType"></param>
    /// <param name="ruleValue"></param>
    Attempt<OperationResult?> RemoveRule(IContent content, string ruleType, string ruleValue);

    /// <summary>
    ///     Saves the entry
    /// </summary>
    /// <param name="entry"></param>
    Attempt<OperationResult?> Save(PublicAccessEntry entry);

    /// <summary>
    ///     Deletes the entry and all associated rules
    /// </summary>
    /// <param name="entry"></param>
    Attempt<OperationResult?> Delete(PublicAccessEntry entry);
}
