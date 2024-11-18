using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Provides a base class for tree entities.
/// </summary>
public abstract class TreeEntityBase : EntityBase, ITreeEntity
{
    private int _creatorId;
    private bool _hasParentId;
    private int _level;
    private string _name = null!;
    private ITreeEntity? _parent;
    private int _parentId;
    private string _path = string.Empty;
    private int _sortOrder;
    private bool _trashed;

    /// <inheritdoc />
    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <inheritdoc />
    [DataMember]
    public int CreatorId
    {
        get => _creatorId;
        set => SetPropertyValueAndDetectChanges(value, ref _creatorId, nameof(CreatorId));
    }

    /// <inheritdoc />
    [DataMember]
    public int ParentId
    {
        get
        {
            if (_hasParentId)
            {
                return _parentId;
            }

            if (_parent == null)
            {
                throw new InvalidOperationException("Content does not have a parent.");
            }

            if (!_parent.HasIdentity)
            {
                throw new InvalidOperationException("Content's parent does not have an identity.");
            }

            _parentId = _parent.Id;
            if (_parentId == 0)
            {
                throw new Exception("Panic: parent has an identity but id is zero.");
            }

            _hasParentId = true;
            _parent = null;
            return _parentId;
        }

        set
        {
            if (value == 0)
            {
                throw new ArgumentException("Value cannot be zero.", nameof(value));
            }

            SetPropertyValueAndDetectChanges(value, ref _parentId, nameof(ParentId));
            _hasParentId = true;
            _parent = null;
        }
    }

    /// <inheritdoc />
    [DataMember]
    public int Level
    {
        get => _level;
        set => SetPropertyValueAndDetectChanges(value, ref _level, nameof(Level));
    }

    /// <inheritdoc />
    public void SetParent(ITreeEntity? parent)
    {
        _hasParentId = false;
        _parent = parent;
        OnPropertyChanged(nameof(ParentId));
    }

    /// <inheritdoc />
    [DataMember]
    public string Path
    {
        get => _path;
        set => SetPropertyValueAndDetectChanges(value, ref _path!, nameof(Path));
    }

    /// <inheritdoc />
    [DataMember]
    public int SortOrder
    {
        get => _sortOrder;
        set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
    }

    /// <inheritdoc />
    [DataMember]
    public bool Trashed
    {
        get => _trashed;
        set => SetPropertyValueAndDetectChanges(value, ref _trashed, nameof(Trashed));
    }
}
