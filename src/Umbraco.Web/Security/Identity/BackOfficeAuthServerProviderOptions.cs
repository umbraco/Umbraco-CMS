namespace Umbraco.Web.Security.Identity
{
    public sealed class BackOfficeAuthServerProviderOptions
    {
        public BackOfficeAuthServerProviderOptions()
        {
            AllowCors = false;
        }

        /// <summary>
        /// True or false to allow CORS (Cross origin resource sharing)
        /// </summary>
        public bool AllowCors { get; set; }
    }
}