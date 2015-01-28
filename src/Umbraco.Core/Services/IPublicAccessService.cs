using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Adds/updates a rule, if an entry doesn't exist one will be created with the new rule
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        /// <returns></returns>
        PublicAccessEntry AddOrUpdateRule(IContent content, string ruleType, string ruleValue);

        /// <summary>
        /// Removes a rule
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        void RemoveRule(IContent content, string ruleType, string ruleValue);

        /// <summary>
        /// Saves the entry
        /// </summary>
        /// <param name="entry"></param>
        void Save(PublicAccessEntry entry);

        /// <summary>
        /// Deletes the entry and all associated rules
        /// </summary>
        /// <param name="entry"></param>
        void Delete(PublicAccessEntry entry);

    }
}