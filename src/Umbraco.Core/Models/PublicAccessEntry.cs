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
        private Guid _protectedNodeId;
        private Guid _noAccessNodeId;
        private Guid _loginNodeId;
        private readonly List<Guid> _removedRules = new List<Guid>();

        public PublicAccessEntry(IContent protectedNode, IContent loginNode, IContent noAccessNode, IEnumerable<PublicAccessRule> ruleCollection)
        {
            LoginNodeId = loginNode.Key;
            NoAccessNodeId = noAccessNode.Key;
            _protectedNodeId = protectedNode.Key;

            _ruleCollection = new ObservableCollection<PublicAccessRule>(ruleCollection);            
            _ruleCollection.CollectionChanged += _ruleCollection_CollectionChanged;
        }

        public PublicAccessEntry(Guid id, Guid protectedNodeId, Guid loginNodeId, Guid noAccessNodeId, IEnumerable<PublicAccessRule> ruleCollection)
        {
            Key = id;

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

        private static readonly PropertyInfo ProtectedNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, Guid>(x => x.ProtectedNodeId);
        private static readonly PropertyInfo LoginNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, Guid>(x => x.LoginNodeId);
        private static readonly PropertyInfo NoAccessNodeIdSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, Guid>(x => x.NoAccessNodeId);
        private static readonly PropertyInfo AllowedSectionsSelector = ExpressionHelper.GetPropertyInfo<PublicAccessEntry, IEnumerable<PublicAccessRule>>(x => x.Rules);

        internal IEnumerable<Guid> RemovedRules
        {
            get { return _removedRules; }
        } 

        public IEnumerable<PublicAccessRule> Rules
        {
            get { return _ruleCollection; }
        }

        public PublicAccessRule AddRule(string claim, string claimType)
        {
            var rule = new PublicAccessRule
            {
                AccessEntryId = Key,
                Claim = claim,
                ClaimType = claimType
            };
            _ruleCollection.Add(rule);
            return rule;
        }

        public void RemoveRule(PublicAccessRule rule)
        {
            _ruleCollection.Remove(rule);
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
        public sealed override Guid Key
        {
            get { return base.Key; }
            set
            {
                base.Key = value;
                HasIdentity = true;
            }
        }

        [DataMember]
        public Guid LoginNodeId
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
        public Guid NoAccessNodeId
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
        public Guid ProtectedNodeId
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