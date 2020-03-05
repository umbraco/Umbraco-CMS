using System;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IKeyValue : IEntity
    {
        string Identifier { get; set; }

        string Value { get; set; }

        DateTime UpdateDate { get; set; }
    }
}
