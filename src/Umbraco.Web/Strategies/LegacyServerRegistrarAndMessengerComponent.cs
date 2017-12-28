using System;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using LightInject;
using Umbraco.Web.Runtime;

namespace Umbraco.Web.Strategies
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class LegacyServerRegistrarAndMessengerComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled == false) return;

            composition.SetServerMessenger(factory =>
            {
                var runtime = factory.GetInstance<IRuntimeState>();
                var logger = factory.GetInstance<ILogger>();
                var userService = factory.GetInstance<IUserService>();

                return new BatchedWebServiceServerMessenger(() =>
                {
                    // we should not proceed to change this if the app/database is not configured since there will
                    // be no user, plus we don't need to have server messages sent if this is the case.
                    if (runtime.Level == RuntimeLevel.Run)
                    {
                        try
                        {
                            var user = userService.GetUserById(UmbracoConfig.For.UmbracoSettings().DistributedCall.UserId);
                            return Tuple.Create(user.Username, user.RawPasswordValue);
                        }
                        catch (Exception e)
                        {
                            logger.Error<WebRuntime>("An error occurred trying to set the IServerMessenger during application startup", e);
                            return null;
                        }
                    }
                    logger.Warn<WebRuntime>("Could not initialize the DefaultServerMessenger, the application is not configured or the database is not configured");
                    return null;
                });
            });
        }
    }
}
