using System;
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