using System;
using System.Reflection;
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
        private static PropertySelectors _selectors;

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

        private static PropertySelectors Selectors => _selectors ?? (_selectors = new PropertySelectors());

        private class PropertySelectors
        {
            public readonly PropertyInfo Group = ExpressionHelper.GetPropertyInfo<Tag, string>(x => x.Group);
            public readonly PropertyInfo Text = ExpressionHelper.GetPropertyInfo<Tag, string>(x => x.Text);
            public readonly PropertyInfo LanguageId = ExpressionHelper.GetPropertyInfo<Tag, int?>(x => x.LanguageId);
        }

        /// <inheritdoc />
        public string Group
        {
            get => _group;
            set => SetPropertyValueAndDetectChanges(value, ref _group, Selectors.Group);
        }

        /// <inheritdoc />
        public string Text
        {
            get => _text;
            set => SetPropertyValueAndDetectChanges(value, ref _text, Selectors.Text);
        }

        /// <inheritdoc />
        public int? LanguageId
        {
            get => _languageId;
            set => SetPropertyValueAndDetectChanges(value, ref _languageId, Selectors.LanguageId);
        }

        /// <inheritdoc />
        public int NodeCount { get; internal set; }
    }
}
