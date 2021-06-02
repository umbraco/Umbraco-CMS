using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Collections;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class PublicAccessEntry : EntityBase
    {
        private readonly EventClearingObservableCollection<PublicAccessRule> _ruleCollection;
        private int _protectedNodeId;
        private int _noAccessNodeId;
        private int _loginNodeId;
        private readonly List<Guid> _removedRules = new List<Guid>();

        public PublicAccessEntry(IContent protectedNode, IContent loginNode, IContent noAccessNode, IEnumerable<PublicAccessRule> ruleCollection)
        {
            if (protectedNode == null) throw new ArgumentNullException(nameof(protectedNode));
            if (loginNode == null) throw new ArgumentNullException(nameof(loginNode));
            if (noAccessNode == null) throw new ArgumentNullException(nameof(noAccessNode));

            LoginNodeId = loginNode.Id;
            NoAccessNodeId = noAccessNode.Id;
            _protectedNodeId = protectedNode.Id;

            _ruleCollection = new EventClearingObservableCollection<PublicAccessRule>(ruleCollection);
            _ruleCollection.CollectionChanged += _ruleCollection_CollectionChanged;

            foreach (var rule in _ruleCollection)
                rule.AccessEntryId = Key;
        }

        public PublicAccessEntry(Guid id, int protectedNodeId, int loginNodeId, int noAccessNodeId, IEnumerable<PublicAccessRule> ruleCollection)
        {
            Key = id;
            Id = Key.GetHashCode();

            LoginNodeId = loginNodeId;
            NoAccessNodeId = noAccessNodeId;
            _protectedNodeId = protectedNodeId;

            _ruleCollection = new EventClearingObservableCollection<PublicAccessRule>(ruleCollection);
            _ruleCollection.CollectionChanged += _ruleCollection_CollectionChanged;

            foreach (var rule in _ruleCollection)
                rule.AccessEntryId = Key;
        }

        void _ruleCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Rules));

            //if (e.Action == NotifyCollectionChangedAction.Add)
            //{
            //    var item = e.NewItems.Cast<PublicAccessRule>().First();

            //    if (_addedSections.Contains(item) == false)
            //    {
            //        _addedSections.Add(item);
            //    }
            //}

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = e.OldItems.Cast<PublicAccessRule>().First();

                if (_removedRules.Contains(item.Key) == false)
                {
                    _removedRules.Add(item.Key);
                }

            }
        }
        
        internal IEnumerable<Guid> RemovedRules => _removedRules;

        public IEnumerable<PublicAccessRule> Rules => _ruleCollection;

        public PublicAccessRule AddRule(string ruleValue, string ruleType)
        {
            var rule = new PublicAccessRule
            {
                AccessEntryId = Key,
                RuleValue = ruleValue,
                RuleType = ruleType
            };
            _ruleCollection.Add(rule);
            return rule;
        }

        public void RemoveRule(PublicAccessRule rule)
        {
            _ruleCollection.Remove(rule);
        }

        public void ClearRules()
        {
            _ruleCollection.Clear();
        }


        internal void ClearRemovedRules()
        {
            _removedRules.Clear();
        }

        [DataMember]
        public int LoginNodeId
        {
            get => _loginNodeId;
            set => SetPropertyValueAndDetectChanges(value, ref _loginNodeId, nameof(LoginNodeId));
        }

        [DataMember]
        public int NoAccessNodeId
        {
            get => _noAccessNodeId;
            set => SetPropertyValueAndDetectChanges(value, ref _noAccessNodeId, nameof(NoAccessNodeId));
        }

        [DataMember]
        public int ProtectedNodeId
        {
            get => _protectedNodeId;
            set => SetPropertyValueAndDetectChanges(value, ref _protectedNodeId, nameof(ProtectedNodeId));
        }

        public override void ResetDirtyProperties(bool rememberDirty)
        {
            _removedRules.Clear();
            base.ResetDirtyProperties(rememberDirty);
            foreach (var publicAccessRule in _ruleCollection)
            {
                publicAccessRule.ResetDirtyProperties(rememberDirty);
            }
        }

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var cloneEntity = (PublicAccessEntry)clone;

            if (cloneEntity._ruleCollection != null)
            {
                cloneEntity._ruleCollection.ClearCollectionChangedEvents();       //clear this event handler if any
                cloneEntity._ruleCollection.CollectionChanged += cloneEntity._ruleCollection_CollectionChanged; //re-assign correct event handler
            }
        }
    }
}
