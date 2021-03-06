using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class PublicAccessService : ScopeRepositoryService, IPublicAccessService
    {
        private readonly IPublicAccessRepository _publicAccessRepository;

        public PublicAccessService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IPublicAccessRepository publicAccessRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _publicAccessRepository = publicAccessRepository;
        }

        /// <summary>
        /// Gets all defined entries and associated rules
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PublicAccessEntry> GetAll()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _publicAccessRepository.GetMany();
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
            var ids = contentPath.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out int val) ? val : -1)
                .Where(x => x != -1)
                .ToList();

            //start with the deepest id
            ids.Reverse();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                //This will retrieve from cache!
                var entries = _publicAccessRepository.GetMany().ToList();
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
        public Attempt<OperationResult<OperationResultType, PublicAccessEntry>> AddRule(IContent content, string ruleType, string ruleValue)
        {
            var evtMsgs = EventMessagesFactory.Get();
            PublicAccessEntry entry;
            using (var scope = ScopeProvider.CreateScope())
            {
                entry = _publicAccessRepository.GetMany().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
                if (entry == null)
                    return OperationResult.Attempt.Cannot<PublicAccessEntry>(evtMsgs); // causes rollback // causes rollback

                var existingRule = entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
                if (existingRule == null)
                {
                    entry.AddRule(ruleValue, ruleType);
                }
                else
                {
                    //If they are both the same already then there's nothing to update, exit
                    //If they are both the same already then there's nothing to update, exit
                    return OperationResult.Attempt.Succeed(evtMsgs, entry);
                }

                var saveEventArgs = new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs, entry);
                }

                _publicAccessRepository.Save(entry);

                scope.Complete();

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
            }

            return OperationResult.Attempt.Succeed(evtMsgs, entry);
        }

        /// <summary>
        /// Removes a rule
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ruleType"></param>
        /// <param name="ruleValue"></param>
        public Attempt<OperationResult> RemoveRule(IContent content, string ruleType, string ruleValue)
        {
            var evtMsgs = EventMessagesFactory.Get();
            PublicAccessEntry entry;
            using (var scope = ScopeProvider.CreateScope())
            {
                entry = _publicAccessRepository.GetMany().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
                if (entry == null) return Attempt<OperationResult>.Fail(); // causes rollback // causes rollback

                var existingRule = entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
                if (existingRule == null) return Attempt<OperationResult>.Fail(); // causes rollback // causes rollback

                entry.RemoveRule(existingRule);

                var saveEventArgs = new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _publicAccessRepository.Save(entry);
                scope.Complete();

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Saves the entry
        /// </summary>
        /// <param name="entry"></param>
        public Attempt<OperationResult> Save(PublicAccessEntry entry)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<PublicAccessEntry>(entry, evtMsgs);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _publicAccessRepository.Save(entry);
                scope.Complete();

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Deletes the entry and all associated rules
        /// </summary>
        /// <param name="entry"></param>
        public Attempt<OperationResult> Delete(PublicAccessEntry entry)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<PublicAccessEntry>(entry, evtMsgs);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _publicAccessRepository.Delete(entry);
                scope.Complete();

                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
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
