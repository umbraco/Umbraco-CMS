// ReSharper disable once CheckNamespace
namespace Umbraco.Core
{
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

        protected virtual void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        { }

        protected virtual void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        { }

        protected virtual void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        { }

        private bool ShouldExecute(ApplicationContext applicationContext)
        {
            if (applicationContext.IsConfigured && applicationContext.DatabaseContext.IsDatabaseConfigured)
            {
                return true;
            }

            if (applicationContext.IsConfigured == false && ExecuteWhenApplicationNotConfigured)
            {
                return true;
            }

            if (applicationContext.DatabaseContext.IsDatabaseConfigured == false && ExecuteWhenDatabaseNotConfigured)
            {
                return true;
            }

            return false;
        }

        protected virtual bool ExecuteWhenApplicationNotConfigured => false;

        protected virtual bool ExecuteWhenDatabaseNotConfigured => false;
    }
}
