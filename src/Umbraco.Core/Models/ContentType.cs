using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the content type that a <see cref="Content"/> object is based on
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class ContentType : ContentTypeCompositionBase, IContentType
    {
        private int _defaultTemplate;
        private IEnumerable<ITemplate> _allowedTemplates;

        /// <summary>
        /// Constuctor for creating a ContentType with the parent's id.
        /// </summary>
        /// <remarks>Only use this for creating ContentTypes at the root (with ParentId -1).</remarks>
        /// <param name="parentId"></param>
        public ContentType(int parentId) : base(parentId)
        {
            _allowedTemplates = new List<ITemplate>();
        }

        /// <summary>
        /// Constuctor for creating a ContentType with the parent as an inherited type.
        /// </summary>
        /// <remarks>Use this to ensure inheritance from parent.</remarks>
        /// <param name="parent"></param>
        [Obsolete("This method is obsolete, use ContentType(IContentType parent, string alias) instead.", false)]
        public ContentType(IContentType parent) : this(parent, null)
        {
        }

        /// <summary>
        /// Constuctor for creating a ContentType with the parent as an inherited type.
        /// </summary>
        /// <remarks>Use this to ensure inheritance from parent.</remarks>
        /// <param name="parent"></param>
        /// <param name="alias"></param>
        public ContentType(IContentType parent, string alias)
            : base(parent, alias)
        {
            _allowedTemplates = new List<ITemplate>();
        }

        private static readonly PropertyInfo DefaultTemplateSelector = ExpressionHelper.GetPropertyInfo<ContentType, int>(x => x.DefaultTemplateId);
        private static readonly PropertyInfo AllowedTemplatesSelector = ExpressionHelper.GetPropertyInfo<ContentType, IEnumerable<ITemplate>>(x => x.AllowedTemplates);

        /// <summary>
        /// Gets or sets the alias of the default Template.
        /// </summary>
        [IgnoreDataMember]
        public ITemplate DefaultTemplate
        {
            get { return AllowedTemplates.FirstOrDefault(x => x != null && x.Id == DefaultTemplateId); }
        }

        /// <summary>
        /// Internal property to store the Id of the default template
        /// </summary>
        [DataMember]
        internal int DefaultTemplateId
        {
            get { return _defaultTemplate; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _defaultTemplate = value;
                    return _defaultTemplate;
                }, _defaultTemplate, DefaultTemplateSelector);
            }
        }

        /// <summary>
        /// Gets or Sets a list of Templates which are allowed for the ContentType
        /// </summary>
        [DataMember]
        public IEnumerable<ITemplate> AllowedTemplates
        {
            get { return _allowedTemplates; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _allowedTemplates = value;
                    return _allowedTemplates;
                }, _allowedTemplates, AllowedTemplatesSelector,
                    //Custom comparer for enumerable
                    new DelegateEqualityComparer<IEnumerable<ITemplate>>(
                        (templates, enumerable) => templates.UnsortedSequenceEqual(enumerable),
                        templates => templates.GetHashCode()));
            }
        }

        /// <summary>
        /// Sets the default template for the ContentType
        /// </summary>
        /// <param name="template">Default <see cref="ITemplate"/></param>
        public void SetDefaultTemplate(ITemplate template)
        {
            if (template == null)
            {
                DefaultTemplateId = 0;
                return;
            }

            DefaultTemplateId = template.Id;
            if (_allowedTemplates.Any(x => x != null && x.Id == template.Id) == false)
            {
                var templates = AllowedTemplates.ToList();
                templates.Add(template);
                AllowedTemplates = templates;
            }
        }

        /// <summary>
        /// Removes a template from the list of allowed templates
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to remove</param>
        /// <returns>True if template was removed, otherwise False</returns>
        public bool RemoveTemplate(ITemplate template)
        {
            if (DefaultTemplateId == template.Id)
                DefaultTemplateId = default(int);

            var templates = AllowedTemplates.ToList();
            var remove = templates.FirstOrDefault(x => x.Id == template.Id);
            var result = templates.Remove(remove);
            AllowedTemplates = templates;

            return result;
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if (Key == Guid.Empty)
                Key = Guid.NewGuid();
        }


        /// <summary>
        /// Creates a deep clone of the current entity with its identity/alias and it's property identities reset
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use DeepCloneWithResetIdentities instead")]
        public IContentType Clone(string alias)
        {
            return DeepCloneWithResetIdentities(alias);
        }

        /// <summary>
        /// Creates a deep clone of the current entity with its identity/alias and it's property identities reset
        /// </summary>
        /// <returns></returns>
        public IContentType DeepCloneWithResetIdentities(string alias)
        {
            var clone = (ContentType)DeepClone();
            clone.Alias = alias;
            clone.Key = Guid.Empty;
            foreach (var propertyGroup in clone.PropertyGroups)
            {
                propertyGroup.ResetIdentity();
                propertyGroup.ResetDirtyProperties(false);
            }
            foreach (var propertyType in clone.PropertyTypes)
            {
                propertyType.ResetIdentity();
                propertyType.ResetDirtyProperties(false);
            }

            clone.ResetIdentity();
            clone.ResetDirtyProperties(false);
            return clone;
        }

    }
}