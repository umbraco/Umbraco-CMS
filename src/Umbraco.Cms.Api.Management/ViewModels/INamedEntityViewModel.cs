namespace Umbraco.Cms.Api.Management.ViewModels;

public interface INamedEntityViewModel
{
    Guid Key { get; }

    string Name { get;}
}
