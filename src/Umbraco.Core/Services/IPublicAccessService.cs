using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models;
using Umbraco.Core.Security;

namespace Umbraco.Core.Services
{
    public interface IPublicAccessService : IService
    {

        /// <summary>
        /// Gets all defined entries and associated rules
        /// </summary>
        /// <returns></returns>
        IEnumerable<PublicAccessEntry> GetAll();

        /// <summary>
        /// Gets the entry defined for the content item's path
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Returns null if no entry is found</returns>
        PublicAccessEntry GetEntryForContent(IContent content);
        
        /// <summary>
        /// Gets the entry defined for the content item based on a content path
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns>Returns null if no entry is found</returns>
        PublicAccessEntry GetEntryForContent(string contentPath);

        /// <summary>
        /// Returns true if the content has an entry for it's path
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        Attempt<PublicAccessEntry> IsProtected(IContent content);

        /// <summary>
        /// Returns true if the content has an entry based on a content path
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        Attempt<PublicAccessEntry> IsProtected(string contentPath);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use AddRule instead, this method will be removed in future versions")]
        Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>> AddOrUpdateRule(IContent content, string ruleType, string ruleValue);

        /// <summary>
        /// Adds a rule if the entry doesn't already exist
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        /// <returns></returns>
        Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>> AddRule(IContent content, string ruleType, string ruleValue);

        /// <summary>
        /// Removes a rule
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        Attempt<OperationStatus> RemoveRule(IContent content, string ruleType, string ruleValue);

        /// <summary>
        /// Saves the entry
        /// </summary>
        /// <param name="entry"></param>
        Attempt<OperationStatus> Save(PublicAccessEntry entry);

        /// <summary>
        /// Deletes the entry and all associated rules
        /// </summary>
        /// <param name="entry"></param>
        Attempt<OperationStatus> Delete(PublicAccessEntry entry);

    }
}