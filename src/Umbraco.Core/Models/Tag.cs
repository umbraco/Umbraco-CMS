using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Tag : Entity, ITag
    {
        public Tag()
        {            
        }

        public Tag(int id, string text, string @group)
        {
            Id = id;
            Text = text;
            Group = @group;
        }

        public Tag(int id, string text, string @group, int nodeCount)
            : this(id, text, @group)
        {
            NodeCount = nodeCount;            
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo TextSelector = ExpressionHelper.GetPropertyInfo<Tag, string>(x => x.Text);
            public readonly PropertyInfo GroupSelector = ExpressionHelper.GetPropertyInfo<Tag, string>(x => x.Group);
        }

        private string _text;
        private string _group;

        public string Text
        {
            get { return _text; }
            set { SetPropertyValueAndDetectChanges(value, ref _text, Ps.Value.TextSelector); }
        }

        public string Group
        {
            get { return _group; }
            set { SetPropertyValueAndDetectChanges(value, ref _group, Ps.Value.GroupSelector); }
        }

        public int NodeCount { get; internal set; }

        //TODO: enable this at some stage
        //public int ParentId { get; set; }
    }
}