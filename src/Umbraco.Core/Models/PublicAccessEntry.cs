using System.Collections.Specialized;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a public access entry that defines protection rules for a content node.
/// </summary>
/// <remarks>
///     Public access entries define which content nodes are protected, which node serves as the login page,
///     which node is displayed when access is denied, and the rules that determine access permissions.
/// </remarks>
[Serializable]
[DataContract(IsReference = true)]
public class PublicAccessEntry : EntityBase
{
    private readonly List<Guid> _removedRules = new();
    private EventClearingObservableCollection<PublicAccessRule> _ruleCollection;
    private int _loginNodeId;
    private int _noAccessNodeId;
    private int _protectedNodeId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessEntry" /> class.
    /// </summary>
    /// <param name="protectedNode">The content node to protect.</param>
    /// <param name="loginNode">The content node to redirect to for login.</param>
    /// <param name="noAccessNode">The content node to display when access is denied.</param>
    /// <param name="ruleCollection">The collection of access rules.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the required parameters is null.</exception>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessEntry" /> class with explicit identifiers.
    /// </summary>
    /// <param name="id">The unique identifier for this entry.</param>
    /// <param name="protectedNodeId">The identifier of the protected content node.</param>
    /// <param name="loginNodeId">The identifier of the login content node.</param>
    /// <param name="noAccessNodeId">The identifier of the no-access content node.</param>
    /// <param name="ruleCollection">The collection of access rules.</param>
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

    /// <summary>
    ///     Gets the keys of rules that have been removed from this entry.
    /// </summary>
    public IEnumerable<Guid> RemovedRules => _removedRules;

    /// <summary>
    ///     Gets the collection of access rules for this entry.
    /// </summary>
    public IEnumerable<PublicAccessRule> Rules => _ruleCollection;

    /// <summary>
    ///     Gets or sets the identifier of the login content node.
    /// </summary>
    [DataMember]
    public int LoginNodeId
    {
        get => _loginNodeId;
        set => SetPropertyValueAndDetectChanges(value, ref _loginNodeId, nameof(LoginNodeId));
    }

    /// <summary>
    ///     Gets or sets the identifier of the content node displayed when access is denied.
    /// </summary>
    [DataMember]
    public int NoAccessNodeId
    {
        get => _noAccessNodeId;
        set => SetPropertyValueAndDetectChanges(value, ref _noAccessNodeId, nameof(NoAccessNodeId));
    }

    /// <summary>
    ///     Gets or sets the identifier of the protected content node.
    /// </summary>
    [DataMember]
    public int ProtectedNodeId
    {
        get => _protectedNodeId;
        set => SetPropertyValueAndDetectChanges(value, ref _protectedNodeId, nameof(ProtectedNodeId));
    }

    /// <summary>
    ///     Adds a new access rule to this entry.
    /// </summary>
    /// <param name="ruleValue">The value of the rule (e.g., a member group name or username).</param>
    /// <param name="ruleType">The type of the rule (e.g., "RoleName" or "Username").</param>
    /// <returns>The newly created access rule.</returns>
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

    /// <summary>
    ///     Removes an access rule from this entry.
    /// </summary>
    /// <param name="rule">The rule to remove.</param>
    public void RemoveRule(PublicAccessRule rule) => _ruleCollection.Remove(rule);

    /// <summary>
    ///     Removes all access rules from this entry.
    /// </summary>
    public void ClearRules() => _ruleCollection.Clear();

    /// <inheritdoc />
    public override void ResetDirtyProperties(bool rememberDirty)
    {
        _removedRules.Clear();
        base.ResetDirtyProperties(rememberDirty);
        foreach (PublicAccessRule publicAccessRule in _ruleCollection)
        {
            publicAccessRule.ResetDirtyProperties(rememberDirty);
        }
    }

    /// <summary>
    ///     Clears the list of removed rules.
    /// </summary>
    internal void ClearRemovedRules() => _removedRules.Clear();

    /// <inheritdoc />
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
