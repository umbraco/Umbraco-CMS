namespace Umbraco.Cms.Core.Services.Changes;

public enum DomainChangeTypes : byte
{
    None = 0,
    RefreshAll = 1,
    Refresh = 2,
    Remove = 3,
}
