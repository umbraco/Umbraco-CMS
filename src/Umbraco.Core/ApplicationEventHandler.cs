namespace Umbraco.Core
{
    /// <summary>
    /// A plugin type that allows developers to execute code during the Umbraco bootup process
    /// </summary>
    /// <remarks>
    /// Allows you to override the methods that you would like to execute code for: ApplicationInitialized, ApplicationStarting, ApplicationStarted.
    /// 
    /// By default none of these methods will execute if the Umbraco application is not configured or if the Umbraco database is not configured, however
    /// if you need these methods to execute even if either of these are not configured you can override the properties: 
    /// ExecuteWhenApplicationNotConfigured and ExecuteWhenDatabaseNotConfigured
    /// </remarks>
    public abstract class ApplicationEventHandler : IApplicationEventHandler
    {
        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (ShouldExecute(applicationContext))
            {
                ApplicationInitialized(umbracoApplication, applicationContext);
            }
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (ShouldExecute(applicationContext))
            {
                ApplicationStarting(umbracoApplication, applicationContext);
            }
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (ShouldExecute(applicationContext))
            {
                ApplicationStarted(umbracoApplication, applicationContext);
            }
        }

        /// <summary>
        /// Overridable method to execute when the ApplicationContext is created and other static objects that require initialization have been setup
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected virtual void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            
        }

        /// <summary>
        /// Overridable method to execute when All resolvers have been initialized but resolution is not frozen so they can be modified in this method
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected virtual void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {

        }

        /// <summary>
        /// Overridable method to execute when Bootup is completed, this allows you to perform any other bootup logic required for the application.
        /// Resolution is frozen so now they can be used to resolve instances.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected virtual void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {

        }

        /// <summary>
        /// Determine if the methods should execute based on the configuration status of the application.
        /// </summary>
        /// <param name="applicationContext"></param>
        /// <returns></returns>
        private bool ShouldExecute(ApplicationContext applicationContext)
        {
            if (applicationContext.IsConfigured && applicationContext.DatabaseContext.IsDatabaseConfigured)
            {
                return true;
            }

            if (!applicationContext.IsConfigured && ExecuteWhenApplicationNotConfigured)
            {
                return true;
            }

            if (!applicationContext.DatabaseContext.IsDatabaseConfigured && ExecuteWhenDatabaseNotConfigured)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// A flag to determine if the overridable methods for this class will execute even if the 
        /// Umbraco application is not configured
        /// </summary>
        /// <remarks>
        /// An Umbraco Application is not configured when it requires a new install or upgrade. When the latest version in the
        /// assembly does not match the version in the config.
        /// </remarks>
        protected virtual bool ExecuteWhenApplicationNotConfigured
        {
            get { return false; }
        }

        /// <summary>
        /// A flag to determine if the overridable methods for this class will execute even if the 
        /// Umbraco database is not configured
        /// </summary>
        /// <remarks>
        /// The Umbraco database is not configured when we cannot connect to the database or when the database tables are not installed.
        /// </remarks>
        protected virtual bool ExecuteWhenDatabaseNotConfigured
        {
            get { return false; }
        }
    }
}