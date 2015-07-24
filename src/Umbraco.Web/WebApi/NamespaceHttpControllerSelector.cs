using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Umbraco.Web.WebApi
{
    public class NamespaceHttpControllerSelector : DefaultHttpControllerSelector
    {
        private const string ControllerKey = "controller";
        private readonly HttpConfiguration _configuration;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> _duplicateControllerTypes;

        public NamespaceHttpControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
            _duplicateControllerTypes = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(GetDuplicateControllerTypes);
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var routeData = request.GetRouteData();
            if (routeData == null 
                || routeData.Route == null
                || routeData.Route.DataTokens == null
                || routeData.Route.DataTokens.ContainsKey("Namespaces") == false
                || routeData.Route.DataTokens["Namespaces"] == null)
                return base.SelectController(request);
            
            // Look up controller in route data
            object controllerName;
            routeData.Values.TryGetValue(ControllerKey, out controllerName);
            var controllerNameAsString = controllerName as string;
            if (controllerNameAsString == null) 
                return base.SelectController(request);
            
            //get the currently cached default controllers - this will not contain duplicate controllers found so if
            // this controller is found in the underlying cache we don't need to do anything
            var map = base.GetControllerMapping();
            if (map.ContainsKey(controllerNameAsString)) 
                return base.SelectController(request);
            
            //the cache does not contain this controller because it's most likely a duplicate, 
            // so we need to sort this out ourselves and we can only do that if the namespace token
            // is formatted correctly.
            var namespaces = routeData.Route.DataTokens["Namespaces"] as IEnumerable<string>;
            if (namespaces == null)
                return base.SelectController(request);

            //see if this is in our cache
            foreach (var ns in namespaces)
            {
                HttpControllerDescriptor descriptor;
                if (_duplicateControllerTypes.Value.TryGetValue(string.Format("{0}-{1}", controllerNameAsString, ns), out descriptor))
                {
                    return descriptor;
                }
            }

            return base.SelectController(request);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> GetDuplicateControllerTypes()
        {
            var assembliesResolver = _configuration.Services.GetAssembliesResolver();
            var controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
            var controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);

            var groupedByName = controllerTypes.GroupBy(
                t => t.Name.Substring(0, t.Name.Length - ControllerSuffix.Length),
                StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1);

            var duplicateControllers = groupedByName.ToDictionary(
                g => g.Key,
                g => g.ToLookup(t => t.Namespace ?? String.Empty, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

            var result = new ConcurrentDictionary<string, HttpControllerDescriptor>();

            foreach (var controllerTypeGroup in duplicateControllers)
            {
                foreach (var controllerType in controllerTypeGroup.Value.SelectMany(controllerTypesGrouping => controllerTypesGrouping))
                {
                    result.TryAdd(string.Format("{0}-{1}", controllerTypeGroup.Key, controllerType.Namespace),
                        new HttpControllerDescriptor(_configuration, controllerTypeGroup.Key, controllerType));
                }
            }

            return result;
        }

    }
}