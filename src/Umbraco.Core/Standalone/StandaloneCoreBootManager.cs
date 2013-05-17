using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Mappers;
using umbraco.interfaces;

namespace Umbraco.Core.Standalone
{
    internal class StandaloneCoreBootManager : CoreBootManager
    {
        private readonly IEnumerable<Type> _handlersToAdd;
        private readonly IEnumerable<Type> _handlersToRemove;

        public StandaloneCoreBootManager(UmbracoApplicationBase umbracoApplication, IEnumerable<Type> handlersToAdd, IEnumerable<Type> handlersToRemove)
            : base(umbracoApplication)
        {
            _handlersToAdd = handlersToAdd;
            _handlersToRemove = handlersToRemove;

            // this is only here to ensure references to the assemblies needed for
            // the DataTypesResolver otherwise they won't be loaded into the AppDomain.
            var interfacesAssemblyName = typeof(IDataType).Assembly.FullName;
        }

        protected override void InitializeApplicationEventsResolver()
        {
            base.InitializeApplicationEventsResolver();
            foreach (var type in _handlersToAdd)
                ApplicationEventsResolver.Current.AddType(type);
            foreach (var type in _handlersToRemove)
                ApplicationEventsResolver.Current.RemoveType(type);
        }

        protected override void InitializeResolvers()
        {
            base.InitializeResolvers();

            //Mappers are not resolved, which could be because of a known TypeMapper issue
            MappingResolver.Reset();
            MappingResolver.Current = new MappingResolver(
                () =>
                new List<Type>
                    {
                        typeof (ContentMapper),
                        typeof (ContentTypeMapper),
                        typeof (MediaMapper),
                        typeof (MediaTypeMapper),
                        typeof (DataTypeDefinitionMapper),
                        typeof (UmbracoEntityMapper)
                    });
        }
    }
}