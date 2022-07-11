using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Dictionary Item
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class DictionaryItem : EntityBase, IDictionaryItem
{
    // Custom comparer for enumerable
    private static readonly DelegateEqualityComparer<IEnumerable<IDictionaryTranslation>>
        DictionaryTranslationComparer =
            new(
                (enumerable, translations) => enumerable.UnsortedSequenceEqual(translations),
                enumerable => enumerable.GetHashCode());

    private string _itemKey;
    private Guid? _parentId;
    private IEnumerable<IDictionaryTranslation> _translations;

    public DictionaryItem(string itemKey)
        : this(null, itemKey)
    {
    }

    public DictionaryItem(Guid? parentId, string itemKey)
    {
        _parentId = parentId;
        _itemKey = itemKey;
        _translations = new List<IDictionaryTranslation>();
    }

    public Func<int, ILanguage?>? GetLanguage { get; set; }

    /// <summary>
    ///     Gets or Sets the Parent Id of the Dictionary Item
    /// </summary>
    [DataMember]
    public Guid? ParentId
    {
        get => _parentId;
        set => SetPropertyValueAndDetectChanges(value, ref _parentId, nameof(ParentId));
    }

    /// <summary>
    ///     Gets or sets the Key for the Dictionary Item
    /// </summary>
    [DataMember]
    public string ItemKey
    {
        get => _itemKey;
        set => SetPropertyValueAndDetectChanges(value, ref _itemKey!, nameof(ItemKey));
    }

    /// <summary>
    ///     Gets or sets a list of translations for the Dictionary Item
    /// </summary>
    [DataMember]
    public IEnumerable<IDictionaryTranslation> Translations
    {
        get => _translations;
        set
        {
            IDictionaryTranslation[] asArray = value.ToArray();

            // ensure the language callback is set on each translation
            if (GetLanguage != null)
            {
                foreach (DictionaryTranslation translation in asArray.OfType<DictionaryTranslation>())
                {
                    translation.GetLanguage = GetLanguage;
                }
            }

            SetPropertyValueAndDetectChanges(asArray, ref _translations!, nameof(Translations), DictionaryTranslationComparer);
        }
    }
}
