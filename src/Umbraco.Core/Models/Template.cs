using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;

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
        private int _creatorId;
        private int _level;
        private int _sortOrder;
        private int _parentId;
        private string _masterTemplateAlias;

        private static readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<Template, int>(x => x.CreatorId);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<Template, int>(x => x.Level);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<Template, int>(x => x.SortOrder);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<Template, int>(x => x.ParentId);
        private static readonly PropertyInfo MasterTemplateAliasSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.MasterTemplateAlias);
        
        public Template(string path, string name, string alias)
            : base(path)
        {
            base.Path = path;
            ParentId = -1;
            _name = name.Replace("/", ".").Replace("\\", "");
            _alias = alias.ToSafeAlias();
        }

        [DataMember]
        internal int CreatorId
        {
            get { return _creatorId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _creatorId = value;
                    return _creatorId;
                }, _creatorId, CreatorIdSelector);    
            }
        }

        [DataMember]
        internal int Level
        {
            get { return _level; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _level = value;
                    return _level;
                }, _level, LevelSelector);    
            }
        }

        [DataMember]
        internal int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _sortOrder = value;
                    return _sortOrder;
                }, _sortOrder, SortOrderSelector);    
            }
        }

        [DataMember]
        internal int ParentId
        {
            get { return _parentId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _parentId = value;
                    return _parentId;
                }, _parentId, ParentIdSelector);    
            }
        }
        
        [DataMember]
        internal Lazy<int> MasterTemplateId { get; set; }

        [DataMember]
        internal string MasterTemplateAlias
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
        public override string Alias
        {
            get
            {
                return _alias;
            }
        }

        [DataMember]
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Returns the <see cref="RenderingEngine"/> that corresponds to the template file
        /// </summary>
        /// <returns><see cref="RenderingEngine"/></returns>
        public RenderingEngine GetTypeOfRenderingEngine()
        {
            if(Path.EndsWith("cshtml") || Path.EndsWith("vbhtml"))
                return RenderingEngine.Mvc;

            return RenderingEngine.WebForms;
        }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        public override bool IsValid()
        {
            var exts = new List<string>();
            if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }
            else
            {
                exts.Add(UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages ? "master" : "aspx");
            }

            var dirs = SystemDirectories.Masterpages;
            if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc)
                dirs += "," + SystemDirectories.MvcViews;

            //Validate file
            var validFile = IOHelper.VerifyEditPath(Path, dirs.Split(','));

            //Validate extension
            var validExtension = IOHelper.VerifyFileExtension(Path, exts);

            return validFile && validExtension;
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
            MasterTemplateId = new Lazy<int>(() => masterTemplate.Id);
        }

        public override object DeepClone()
        {
            var clone = (Template)base.DeepClone();

            //need to manually assign since they are readonly properties
            clone._alias = Alias;
            clone._name = Name;

            clone.ResetDirtyProperties(false);

            return clone;
        }
    }
}
