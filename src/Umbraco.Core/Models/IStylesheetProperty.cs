using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IStylesheetProperty : IRememberBeingDirty
    {
        string Alias { get; set; }
        string Name { get;  }
        string Value { get; set; }
    }
}
