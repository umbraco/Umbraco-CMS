using System;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Test : EntityBase, ITest
    {
        private string _name;

        public Test(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }
    }
}
