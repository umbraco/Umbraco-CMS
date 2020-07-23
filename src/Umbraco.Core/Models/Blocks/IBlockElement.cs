namespace Umbraco.Core.Models.Blocks
{

    // TODO: IBlockElement doesn't make sense, this is a reference to an actual element with some settings
    // and always has to do with the "Layout", should possibly be called IBlockReference or IBlockLayout or IBlockLayoutReference
    /// <summary>
    /// Represents a data item for a Block editor implementation
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    /// <remarks>
    /// see: https://github.com/umbraco/rfcs/blob/907f3758cf59a7b6781296a60d57d537b3b60b8c/cms/0011-block-data-structure.md#strongly-typed
    /// </remarks>
    public interface IBlockElement<TSettings> : IBlockReference
    {
        TSettings Settings { get; }
    }
}
