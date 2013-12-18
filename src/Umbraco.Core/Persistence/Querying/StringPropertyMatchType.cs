namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Determines how to match a string property value
    /// </summary>
    public enum StringPropertyMatchType
    {
        Exact,
        Contains,
        StartsWith,
        EndsWith
    }
}