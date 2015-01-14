using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Template file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Template : File, ITemplate
    {
        private string _alias;
        private string _name;
        private string _masterTemplateAlias;
        private Lazy<int> _masterTemplateId;

        private static readonly PropertyInfo MasterTemplateAliasSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.MasterTemplateAlias);
        private static readonly PropertyInfo MasterTemplateIdSelector = ExpressionHelper.GetPropertyInfo<Template, Lazy<int>>(x => x.MasterTemplateId);
        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.Alias);
        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.Name);

        public Template(string name, string alias)
            : base(string.Empty)
        {
            _name = name;
            _alias = alias.ToCleanString(CleanStringType.UnderscoreAlias);
            _masterTemplateId = new Lazy<int>(() => -1);
        }        

        [Obsolete("This constructor should not be used, file path is determined by alias, setting the path here will have no affect")]
        public Template(string path, string name, string alias)
            : this(name, alias)
        {            
        }

        [DataMember]
        public Lazy<int> MasterTemplateId
        {
            get { return _masterTemplateId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _masterTemplateId = value;
                    return _masterTemplateId;
                }, _masterTemplateId, MasterTemplateIdSelector);
            }
        }

        public string MasterTemplateAlias
        {
            get { return _masterTemplateAlias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _masterTemplateAlias = value;
                    return _masterTemplateAlias;
                }, _masterTemplateAlias, MasterTemplateAliasSelector);
            }
        }

        [DataMember]
        public new string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
                
            }
        }

        [DataMember]
        public new string Alias
        {
            get { return _alias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _alias = value.ToCleanString(CleanStringType.UnderscoreAlias);
                    return _alias;
                }, _alias, AliasSelector);
                
            }
        }

        /// <summary>
        /// Returns true if the template is used as a layout for other templates (i.e. it has 'children')
        /// </summary>
        public bool IsMasterTemplate { get; internal set; }

        [Obsolete("This is no longer used and will be removed from the codebase in future versions, use the IFileSystem DetermineRenderingEngine method instead")]
        public RenderingEngine GetTypeOfRenderingEngine()
        {
            //Hack! TODO: Remove this method entirely
            return ApplicationContext.Current.Services.FileService.DetermineTemplateRenderingEngine(this);
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


        public void SetMasterTemplate(ITemplate masterTemplate)
        {
            if (masterTemplate == null)
            {
                MasterTemplateId = new Lazy<int>(() => -1);
                MasterTemplateAlias = null;
            }
            else
            {
                MasterTemplateId = new Lazy<int>(() => masterTemplate.Id);
                MasterTemplateAlias = masterTemplate.Alias;
            }
           
        }

        public override object DeepClone()
        {
            //We cannot call in to the base classes to clone because the base File class treats Alias, Name.. differently so we need to manually do the clone

            //Memberwise clone on Entity will work since it doesn't have any deep elements
            // for any sub class this will work for standard properties as well that aren't complex object's themselves.
            var clone = (Template)MemberwiseClone();
            //Automatically deep clone ref properties that are IDeepCloneable
            DeepCloneHelper.DeepCloneRefProperties(this, clone);           

            clone.ResetDirtyProperties(false);
            return clone;
        }

        
    }
}
