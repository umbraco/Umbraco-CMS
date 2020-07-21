namespace Umbraco.Core.Models.Blocks
{
    /// <summary>
    /// Represents a data item reference for a Block Editor implementation
    /// </summary>
    /// <remarks>
    /// see: https://github.com/umbraco/rfcs/blob/907f3758cf59a7b6781296a60d57d537b3b60b8c/cms/0011-block-data-structure.md#strongly-typed
    /// </remarks>
    public interface IBlockReference
    {
        Udi ContentUdi { get; }
    }
}
