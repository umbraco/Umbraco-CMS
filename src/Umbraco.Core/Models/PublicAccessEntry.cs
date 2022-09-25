using System.Collections.Specialized;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

[Serializable]
[DataContract(IsReference = true)]
public class PublicAccessEntry : EntityBase
{
    private readonly List<Guid> _removedRules = new();
    private EventClearingObservableCollection<PublicAccessRule> _ruleCollection;
    private int _loginNodeId;
    private int _noAccessNodeId;
    private int _protectedNodeId;

    public PublicAccessEntry(IContent protectedNode, IContent loginNode, IContent noAccessNode, IEnumerable<PublicAccessRule> ruleCollection)
    {
        if (protectedNode == null)
        {
            throw new ArgumentNullException(nameof(protectedNode));
        }

        if (loginNode == null)
        {
            throw new ArgumentNullException(nameof(loginNode));
        }

        if (noAccessNode == null)
        {
            throw new ArgumentNullException(nameof(noAccessNode));
        }

        LoginNodeId = loginNode.Id;
        NoAccessNodeId = noAccessNode.Id;
        _protectedNodeId = protectedNode.Id;

        _ruleCollection = new EventClearingObservableCollection<PublicAccessRule>(ruleCollection);
        _ruleCollection.CollectionChanged += RuleCollection_CollectionChanged;

        foreach (PublicAccessRule rule in _ruleCollection)
        {
            rule.AccessEntryId = Key;
        }
    }

    public PublicAccessEntry(Guid id, int protectedNodeId, int loginNodeId, int noAccessNodeId, IEnumerable<PublicAccessRule> ruleCollection)
    {
        Key = id;
        Id = Key.GetHashCode();

        LoginNodeId = loginNodeId;
        NoAccessNodeId = noAccessNodeId;
        _protectedNodeId = protectedNodeId;

        _ruleCollection = new EventClearingObservableCollection<PublicAccessRule>(ruleCollection);
        _ruleCollection.CollectionChanged += RuleCollection_CollectionChanged;

        foreach (PublicAccessRule rule in _ruleCollection)
        {
            rule.AccessEntryId = Key;
        }
    }

    public IEnumerable<Guid> RemovedRules => _removedRules;

    public IEnumerable<PublicAccessRule> Rules => _ruleCollection;

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

    public PublicAccessRule AddRule(string ruleValue, string ruleType)
    {
        var rule = new PublicAccessRule { AccessEntryId = Key, RuleValue = ruleValue, RuleType = ruleType };
        _ruleCollection.Add(rule);
        return rule;
    }

    private void RuleCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Rules));

        // if (e.Action == NotifyCollectionChangedAction.Add)
        // {
        //    var item = e.NewItems.Cast<PublicAccessRule>().First();

        // if (_addedSections.Contains(item) == false)
        //    {
        //        _addedSections.Add(item);
        //    }
        // }
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            PublicAccessRule? item = e.OldItems?.Cast<PublicAccessRule>().First();

            if (item is not null)
            {
                if (_removedRules.Contains(item.Key) == false)
                {
                    _removedRules.Add(item.Key);
                }
            }
        }
    }

    public void RemoveRule(PublicAccessRule rule) => _ruleCollection.Remove(rule);

    public void ClearRules() => _ruleCollection.Clear();

    public override void ResetDirtyProperties(bool rememberDirty)
    {
        _removedRules.Clear();
        base.ResetDirtyProperties(rememberDirty);
        foreach (PublicAccessRule publicAccessRule in _ruleCollection)
        {
            publicAccessRule.ResetDirtyProperties(rememberDirty);
        }
    }

    internal void ClearRemovedRules() => _removedRules.Clear();

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var cloneEntity = (PublicAccessEntry)clone;

        // clear this event handler if any
        cloneEntity._ruleCollection.ClearCollectionChangedEvents();

        // clone the rule collection explicitly
        cloneEntity._ruleCollection = (EventClearingObservableCollection<PublicAccessRule>)_ruleCollection.DeepClone();

        // re-assign correct event handler
        cloneEntity._ruleCollection.CollectionChanged += cloneEntity.RuleCollection_CollectionChanged;
    }
}
