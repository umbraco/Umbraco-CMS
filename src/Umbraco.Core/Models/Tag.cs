using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public class TaggedEntity
    {
        public TaggedEntity(int entityId, IEnumerable<TaggedProperty> taggedProperties)
        {
            EntityId = entityId;
            TaggedProperties = taggedProperties;
        }

        public int EntityId { get; private set; }
        public IEnumerable<TaggedProperty> TaggedProperties { get; private set; }
    }

    public class TaggedProperty
    {
        public TaggedProperty(int propertyTypeId, string propertyTypeAlias, IEnumerable<Tag> tags)
        {
            PropertyTypeId = propertyTypeId;
            PropertyTypeAlias = propertyTypeAlias;
            Tags = tags;
        }

        public int PropertyTypeId { get; private set; }
        public string PropertyTypeAlias { get; private set; }
        public IEnumerable<Tag> Tags { get; private set; }
    }

    [Serializable]
    [DataContract(IsReference = true)]
    public class Tag : Entity, ITag
    {
        public Tag()
        {            
        }

        public Tag(int id, string text, string @group)
        {
            Text = text;
            Group = @group;
            Id = id;
        }

        private static readonly PropertyInfo TextSelector = ExpressionHelper.GetPropertyInfo<Tag, string>(x => x.Text);
        private static readonly PropertyInfo GroupSelector = ExpressionHelper.GetPropertyInfo<Tag, string>(x => x.Group);
        private string _text;
        private string _group;

        public string Text
        {
            get { return _text; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _text = value;
                    return _text;
                }, _text, TextSelector);
            }
        }

        public string Group
        {
            get { return _group; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _group = value;
                    return _group;
                }, _group, GroupSelector);
            }
        }

        //TODO: enable this at some stage
        //public int ParentId { get; set; }
    }
}