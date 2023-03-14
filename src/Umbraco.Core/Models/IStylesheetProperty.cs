using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IStylesheetProperty : IRememberBeingDirty
{
    string Alias { get; set; }

    string Name { get; }

    string Value { get; set; }
}
