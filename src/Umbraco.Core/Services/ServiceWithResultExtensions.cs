namespace Umbraco.Core.Services
{
    /// <summary>
    /// These are used currently to return the temporary 'operation' interfaces for services
    /// which are used to return a status from operational methods so we can determine if things are
    /// cancelled, etc...
    ///
    /// These will be obsoleted in v8 since all real services methods will be changed to have the correct result.
    /// </summary>
    public static class ServiceWithResultExtensions
    {
        public static IMediaServiceOperations WithResult(this IMediaService mediaService)
        {
            return (IMediaServiceOperations)mediaService;
        }
    }
}
