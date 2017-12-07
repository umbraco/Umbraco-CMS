namespace Umbraco.Core.Persistence.Repositories
{
    // this only exists to differenciate with IPartialViewRepository in IoC
    // without resorting to constants, names, whatever - and IPartialViewRepository
    // is implemented by PartialViewRepository and IPartialViewMacroRepository by
    // PartialViewMacroRepository - just to inject the proper filesystem.
    internal interface IPartialViewMacroRepository : IPartialViewRepository
    { }
}
