namespace Umbraco.Core.Persistence.Repositories
{
    // this only exists to differenciate with IPartialViewRepository in IoC
    // without resorting to constants, names, whatever - both interfaces are
    // implemented by PartialViewRepository anyway
    // fixme - what about file systems?!
    internal interface IPartialViewMacroRepository : IPartialViewRepository
    { }
}
