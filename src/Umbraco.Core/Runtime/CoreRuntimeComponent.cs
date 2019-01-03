using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Components;
using Umbraco.Core.IO;

namespace Umbraco.Core.Runtime
{
    public class CoreRuntimeComponent : IComponent
    {
        internal CoreRuntimeComponent(IEnumerable<Profile> mapperProfiles)
        {
            // mapper profiles have been registered & are created by the container
            Mapper.Initialize(configuration =>
            {
                foreach (var profile in mapperProfiles)
                    configuration.AddProfile(profile);
            });

            // ensure we have some essential directories
            // every other component can then initialize safely
            IOHelper.EnsurePathExists("~/App_Data");
            IOHelper.EnsurePathExists(SystemDirectories.Media);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");
        }
    }
}
