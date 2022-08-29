namespace Umbraco.Cms.Persistence.EFCore;

public interface IDatabaseDataCreator
{
    Task SeedDataAsync();
}
