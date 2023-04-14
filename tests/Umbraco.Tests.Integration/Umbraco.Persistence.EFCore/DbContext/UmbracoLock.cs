namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

internal class UmbracoLock
{
    public int Id { get; set; }

    public int Value { get; set; } = 1;

    public string Name { get; set; } = null!;
}
