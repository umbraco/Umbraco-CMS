using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class PublicAccessService : RepositoryService, IPublicAccessService
    {
        public PublicAccessService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
        }

        /// <summary>
        /// Gets all defined entries and associated rules
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PublicAccessEntry> GetAll()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IPublicAccessRepository>();
                var entries = repo.GetAll();
                uow.Complete();
                return entries;
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IPublicAccessRepository>();

                //This will retrieve from cache!                 
                var entries = repo.GetAll().ToArray();
                uow.Complete();

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

        /// <summary>
        /// Adds a rule
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        /// <returns></returns>
        public Attempt<OperationStatus<OperationStatusType, PublicAccessEntry>> AddRule(IContent content, string ruleType, string ruleValue)
        {
            var evtMsgs = EventMessagesFactory.Get();
            PublicAccessEntry entry;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IPublicAccessRepository>();

                entry = repo.GetAll().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
                if (entry == null)
                    return OperationStatus.Attempt.Cannot<PublicAccessEntry>(evtMsgs); // causes rollback

                var existingRule = entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
                if (existingRule == null)
                {
                    entry.AddRule(ruleValue, ruleType);
                }
                else
                {
                    //If they are both the same already then there's nothing to update, exit
                    return OperationStatus.Attempt.Succeed(evtMsgs, entry);
                }

                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs), this))
                    return OperationStatus.Attempt.Cancel(evtMsgs, entry); // causes rollback

                repo.AddOrUpdate(entry);

                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
            return OperationStatus.Attempt.Succeed(evtMsgs, entry);
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
            PublicAccessEntry entry;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IPublicAccessRepository>();

                entry = repo.GetAll().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
                if (entry == null) return Attempt<OperationStatus>.Fail(); // causes rollback

                var existingRule = entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
                if (existingRule == null) return Attempt<OperationStatus>.Fail(); // causes rollback

                entry.RemoveRule(existingRule);

                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs), this))
                    return OperationStatus.Attempt.Cancel(evtMsgs); // causes rollback

                repo.AddOrUpdate(entry);
                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
            return OperationStatus.Attempt.Succeed(evtMsgs);
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
                return OperationStatus.Attempt.Cancel(evtMsgs);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IPublicAccessRepository>();
                repo.AddOrUpdate(entry);
                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
            return OperationStatus.Attempt.Succeed(evtMsgs);
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
                return OperationStatus.Attempt.Cancel(evtMsgs);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IPublicAccessRepository>();
                repo.Delete(entry);
                uow.Complete();
            }

            Deleted.RaiseEvent(new DeleteEventArgs<PublicAccessEntry>(entry, false, evtMsgs), this);
            return OperationStatus.Attempt.Succeed(evtMsgs);
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