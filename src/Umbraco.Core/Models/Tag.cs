using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a tag entity.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Tag : EntityBase, ITag
    {
        private string _group;
        private string _text;
        private int? _languageId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag(int id, string group, string text, int? languageId = null)
        {
            Id = id;
            Text = text;
            Group = group;
            LanguageId = languageId;
        }

        /// <inheritdoc />
        public string Group
        {
            get => _group;
            set => SetPropertyValueAndDetectChanges(value, ref _group, nameof(Group));
        }

        /// <inheritdoc />
        public string Text
        {
            get => _text;
            set => SetPropertyValueAndDetectChanges(value, ref _text, nameof(Text));
        }

        /// <inheritdoc />
        public int? LanguageId
        {
            get => _languageId;
            set => SetPropertyValueAndDetectChanges(value, ref _languageId, nameof(LanguageId));
        }

        /// <inheritdoc />
        public int NodeCount { get; internal set; }
    }
}
