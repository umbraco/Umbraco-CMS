using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the contnet type that a <see cref="Content"/> object is based on
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class ContentType : ContentTypeCompositionBase, IContentType
    {
        private string _defaultTemplate;
        private IEnumerable<string> _allowedTemplates;

        public ContentType(int parentId) : base(parentId)
        {
            _allowedTemplates = new List<string>();
        }

        private static readonly PropertyInfo DefaultTemplateSelector = ExpressionHelper.GetPropertyInfo<ContentType, string>(x => x.DefaultTemplate);
        private static readonly PropertyInfo AllowedTemplatesSelector = ExpressionHelper.GetPropertyInfo<ContentType, IEnumerable<string>>(x => x.AllowedTemplates);
        
        /// <summary>
        /// Gets or sets the Path to default Template
        /// </summary>
        [DataMember]
        public string DefaultTemplate
        {
            get { return _defaultTemplate; }
            set
            {
                _defaultTemplate = value;
                OnPropertyChanged(DefaultTemplateSelector);
            }
        }

        /// <summary>
        /// Gets or sets a list of aliases for allowed Templates
        /// </summary>
        public IEnumerable<string> AllowedTemplates
        {
            get { return _allowedTemplates; }
            set
            {
                _allowedTemplates = value;
                OnPropertyChanged(AllowedTemplatesSelector);
            }
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();
            Key = Guid.NewGuid();
        }

        /// <summary>
        /// Method to call when Entity is being updated
        /// </summary>
        /// <remarks>Modified Date is set and a new Version guid is set</remarks>
        internal override void UpdatingEntity()
        {
            base.UpdatingEntity();
        }
    }
}