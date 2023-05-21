using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IKeyValue : IEntity
{
    string Identifier { get; set; }

    string? Value { get; set; }
}
