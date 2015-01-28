using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class PublicAccessEntry : Entity, IAggregateRoot
    {
        private readonly ObservableCollection<PublicAccessRule> _ruleCollection;
        private int _protectedNodeId;
        private int _noAccessNodeId;
        private int _loginNodeId;
        private readonly List<Guid> _removedRules = new List<Guid>();

        public PublicAccessEntry(IContent protectedNode, IContent loginNode, IContent noAccessNode, IEnumerable<PublicAccessRule> ruleCollection)
        {
            LoginNodeId = loginNode.Id;
            NoAccessNodeId = noAccessNode.Id;
            _protectedNodeId = protectedNode.Id;

            _ruleCollection = new ObservableCollection<PublicAccessRule>(ruleCollection);            
            _ruleCollection.CollectionChanged += _ruleCollection_CollectionChanged;
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
        }

        void _ruleCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(AllowedSectionsSelector);

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

        private static readonly PropertyInfo ProtectedNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, int>(x => x.ProtectedNodeId);
        private static readonly PropertyInfo LoginNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, int>(x => x.LoginNodeId);
        private static readonly PropertyInfo NoAccessNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, int>(x => x.NoAccessNodeId);
        private static readonly PropertyInfo AllowedSectionsSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, IEnumerable<PublicAccessRule>>(x => x.Rules);

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
            foreach (var rule in _ruleCollection)
            {
                RemoveRule(rule);
            }
        }

        /// <summary>
        /// Method to call on entity saved when first added
        /// </summary>
        internal override void AddingEntity()
        {
            if (Key == default(Guid))
            {
                Key = Guid.NewGuid();
            }
            base.AddingEntity();
        }

        [DataMember]
        public int LoginNodeId
        {
            get { return _loginNodeId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _loginNodeId = value;
                    return _loginNodeId;
                }, _loginNodeId, LoginNodeIdSelector);
            }
        }

        [DataMember]
        public int NoAccessNodeId
        {
            get { return _noAccessNodeId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _noAccessNodeId = value;
                    return _noAccessNodeId;
                }, _noAccessNodeId, NoAccessNodeIdSelector);
            }
        }
       
        [DataMember]
        public int ProtectedNodeId
        {
            get { return _protectedNodeId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _protectedNodeId = value;
                    return _protectedNodeId;
                }, _protectedNodeId, ProtectedNodeIdSelector);
            }
        }

        public override void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            _removedRules.Clear();
            base.ResetDirtyProperties(rememberPreviouslyChangedProperties);
            foreach (var publicAccessRule in _ruleCollection)
            {
                publicAccessRule.ResetDirtyProperties(rememberPreviouslyChangedProperties);
            }
        }
    }
}