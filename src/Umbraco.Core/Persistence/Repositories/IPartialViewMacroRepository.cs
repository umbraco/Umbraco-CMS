namespace Umbraco.Cms.Core.Persistence.Repositories;

// this only exists to differentiate with IPartialViewRepository in IoC
// without resorting to constants, names, whatever - and IPartialViewRepository
// is implemented by PartialViewRepository and IPartialViewMacroRepository by
// PartialViewMacroRepository - just to inject the proper filesystem.
public interface IPartialViewMacroRepository : IPartialViewRepository
{
}
