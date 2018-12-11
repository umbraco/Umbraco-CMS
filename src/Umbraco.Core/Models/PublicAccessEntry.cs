using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class PublicAccessEntry : EntityBase
    {
        private readonly ObservableCollection<PublicAccessRule> _ruleCollection;
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

            _ruleCollection = new ObservableCollection<PublicAccessRule>(ruleCollection);
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

            _ruleCollection = new ObservableCollection<PublicAccessRule>(ruleCollection);
            _ruleCollection.CollectionChanged += _ruleCollection_CollectionChanged;

            foreach (var rule in _ruleCollection)
                rule.AccessEntryId = Key;
        }

        void _ruleCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.AllowedSectionsSelector);

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

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo ProtectedNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, int>(x => x.ProtectedNodeId);
            public readonly PropertyInfo LoginNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, int>(x => x.LoginNodeId);
            public readonly PropertyInfo NoAccessNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, int>(x => x.NoAccessNodeId);
            public readonly PropertyInfo AllowedSectionsSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, IEnumerable<PublicAccessRule>>(x => x.Rules);
        }

        internal IEnumerable<Guid> RemovedRules
        {
            get { return _removedRules; }
        }

        public IEnumerable<PublicAccessRule> Rules
        {
            get { return _ruleCollection; }
        }

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
            get { return _loginNodeId; }
            set { SetPropertyValueAndDetectChanges(value, ref _loginNodeId, Ps.Value.LoginNodeIdSelector); }
        }

        [DataMember]
        public int NoAccessNodeId
        {
            get { return _noAccessNodeId; }
            set { SetPropertyValueAndDetectChanges(value, ref _noAccessNodeId, Ps.Value.NoAccessNodeIdSelector); }
        }

        [DataMember]
        public int ProtectedNodeId
        {
            get { return _protectedNodeId; }
            set { SetPropertyValueAndDetectChanges(value, ref _protectedNodeId, Ps.Value.ProtectedNodeIdSelector); }
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
                cloneEntity._ruleCollection.CollectionChanged -= _ruleCollection_CollectionChanged;       //clear this event handler if any
                cloneEntity._ruleCollection.CollectionChanged += cloneEntity._ruleCollection_CollectionChanged; //re-assign correct event handler
            }
        }
    }
}
