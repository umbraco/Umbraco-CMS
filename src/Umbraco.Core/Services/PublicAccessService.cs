using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class PublicAccessService : RepositoryService, IPublicAccessService
    {
        public PublicAccessService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
        }

        /// <summary>
        /// Gets all defined entries and associated rules
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PublicAccessEntry> GetAll()
        {
            using (var repo = RepositoryFactory.CreatePublicAccessRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.GetAll();
            }
        }

        /// <summary>
        /// Gets the entry defined for the content item's path
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Returns null if no entry is found</returns>
        public PublicAccessEntry GetEntryForContent(IContent content)
        {
            return GetEntryForContent(content.Path.EnsureEndsWith("," + content.Id));
        }

        /// <summary>
        /// Gets the entry defined for the content item based on a content path
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns>Returns null if no entry is found</returns>
        /// <remarks>
        /// NOTE: This method get's called *very* often! This will return the results from cache
        /// </remarks>
        public PublicAccessEntry GetEntryForContent(string contentPath)
        {
            //Get all ids in the path for the content item and ensure they all
            // parse to ints that are not -1.
            var ids = contentPath.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    int val;
                    if (int.TryParse(x, out val))
                    {
                        return val;
                    }
                    return -1;
                })
                .Where(x => x != -1)
                .ToList();

            //start with the deepest id
            ids.Reverse();

            using (var repo = RepositoryFactory.CreatePublicAccessRepository(UowProvider.GetUnitOfWork()))
            {
                //This will retrieve from cache!                 
                var entries = repo.GetAll().ToArray();

                foreach (var id in ids)
                {
                    var found = entries.FirstOrDefault(x => x.ProtectedNodeId == id);
                    if (found != null) return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the content has an entry for it's path
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public Attempt<PublicAccessEntry> IsProtected(IContent content)
        {
            var result = GetEntryForContent(content);
            return Attempt.If(result != null, result);
        }

        /// <summary>
        /// Returns true if the content has an entry based on a content path
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public Attempt<PublicAccessEntry> IsProtected(string contentPath)
        {
            var result = GetEntryForContent(contentPath);
            return Attempt.If(result != null, result);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use AddRule instead, this method will be removed in future versions")]
        public Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>> AddOrUpdateRule(IContent content, string ruleType, string ruleValue)
        {
            return AddRule(content, ruleType, ruleValue);
        }

        /// <summary>
        /// Adds a rule
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        /// <returns></returns>
        public Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>> AddRule(IContent content, string ruleType, string ruleValue)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreatePublicAccessRepository(uow))
            {
                var entry = repo.GetAll().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
                if (entry == null)
                    return Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>>.Fail();

                var existingRule = entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
                if (existingRule == null)
                {
                    entry.AddRule(ruleValue, ruleType);
                }
                else
                {
                    //If they are both the same already then there's nothing to update, exit
                    return Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>>.Succeed(
                        new OperationStatus<PublicAccessEntry, OperationStatusType>(entry, OperationStatusType.Success, evtMsgs));
                }

                if (Saving.IsRaisedEventCancelled(
                    new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs),
                    this))
                {
                    return Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>>.Fail(
                        new OperationStatus<PublicAccessEntry, OperationStatusType>(entry, OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                repo.AddOrUpdate(entry);

                uow.Commit();

                Saved.RaiseEvent(new SaveEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
                return Attempt<OperationStatus<PublicAccessEntry, OperationStatusType>>.Succeed(
                    new OperationStatus<PublicAccessEntry, OperationStatusType>(entry, OperationStatusType.Success, evtMsgs));
            }
        }

        /// <summary>
        /// Removes a rule
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        public Attempt<OperationStatus> RemoveRule(IContent content, string ruleType, string ruleValue)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreatePublicAccessRepository(uow))
            {
                var entry = repo.GetAll().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
                if (entry == null) return Attempt<OperationStatus>.Fail();

                var existingRule = entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
                if (existingRule == null) return Attempt<OperationStatus>.Fail();

                entry.RemoveRule(existingRule);

                if (Saving.IsRaisedEventCancelled(
                    new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs),
                    this))
                {
                    return OperationStatus.Cancelled(evtMsgs);
                }

                repo.AddOrUpdate(entry);

                uow.Commit();

                Saved.RaiseEvent(new SaveEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
                return OperationStatus.Success(evtMsgs);
            }

        }

        /// <summary>
        /// Saves the entry
        /// </summary>
        /// <param name="entry"></param>
        public Attempt<OperationStatus> Save(PublicAccessEntry entry)
        {
            var evtMsgs = EventMessagesFactory.Get();
            if (Saving.IsRaisedEventCancelled(
                    new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs),
                    this))
            {
                return OperationStatus.Cancelled(evtMsgs);
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreatePublicAccessRepository(uow))
            {
                repo.AddOrUpdate(entry);
                uow.Commit();
            }

            Saved.RaiseEvent(new SaveEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
            return OperationStatus.Success(evtMsgs);
        }

        /// <summary>
        /// Deletes the entry and all associated rules
        /// </summary>
        /// <param name="entry"></param>
        public Attempt<OperationStatus> Delete(PublicAccessEntry entry)
        {
            var evtMsgs = EventMessagesFactory.Get();
            if (Deleting.IsRaisedEventCancelled(
                    new DeleteEventArgs<PublicAccessEntry>(entry, evtMsgs),
                    this))
            {
                return OperationStatus.Cancelled(evtMsgs);
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreatePublicAccessRepository(uow))
            {
                repo.Delete(entry);
                uow.Commit();
            }

            Deleted.RaiseEvent(new DeleteEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
            return OperationStatus.Success(evtMsgs);
        }

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IPublicAccessService, SaveEventArgs<PublicAccessEntry>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IPublicAccessService, SaveEventArgs<PublicAccessEntry>> Saved;

        /// <summary>
        /// Occurs before Delete
        /// </summary>		
        public static event TypedEventHandler<IPublicAccessService, DeleteEventArgs<PublicAccessEntry>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IPublicAccessService, DeleteEventArgs<PublicAccessEntry>> Deleted;


    }
}