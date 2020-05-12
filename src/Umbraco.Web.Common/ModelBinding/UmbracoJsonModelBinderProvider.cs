using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Web.Common.Controllers;

namespace Umbraco.Web.Common.ModelBinding
{

    public class UmbracoJsonModelBinderFactory : ModelBinderFactory
    {
        public UmbracoJsonModelBinderFactory(
            UmbracoJsonModelBinderProvider umbracoJsonModelBinderProvider,
            IModelMetadataProvider metadataProvider,
            IOptions<MvcOptions> options,
            IServiceProvider serviceProvider)
            : base(metadataProvider, GetOptions(options, umbracoJsonModelBinderProvider), serviceProvider)
        {
        }

        private static IOptions<MvcOptions> GetOptions(IOptions<MvcOptions> options, UmbracoJsonModelBinderProvider umbracoJsonModelBinderProvider)
        {
            // copy to new collection
            var providers = options.Value.ModelBinderProviders.ToList();            
            // remove the default
            providers.RemoveType<BodyModelBinderProvider>();
            // prepend our own
            providers.Insert(0, umbracoJsonModelBinderProvider);
            var newOptions = new MvcOptions();
            foreach (var p in providers)
                newOptions.ModelBinderProviders.Add(p);
            return new CustomOptions(newOptions);
        }

        private class CustomOptions : IOptions<MvcOptions>
        {
            public CustomOptions(MvcOptions options)
            {
                Value = options;
            }
            public MvcOptions Value { get; }
        }
    }

    public class UmbracoJsonModelBinder : BodyModelBinder, IModelBinder
    {
        public UmbracoJsonModelBinder(ArrayPool<char> arrayPool, ObjectPoolProvider objectPoolProvider, IHttpRequestStreamReaderFactory readerFactory, ILoggerFactory loggerFactory)
            : base(GetNewtonsoftJsonFormatter(loggerFactory, arrayPool, objectPoolProvider), readerFactory, loggerFactory)
        {
        }

        internal static IInputFormatter[] GetNewtonsoftJsonFormatter(ILoggerFactory logger, ArrayPool<char> arrayPool, ObjectPoolProvider objectPoolProvider)
        {
            var jsonOptions = new MvcNewtonsoftJsonOptions
            {
                AllowInputFormatterExceptionMessages = true
            };
            return new IInputFormatter[]
            {
                new NewtonsoftJsonInputFormatter(
                    logger.CreateLogger<UmbracoJsonModelBinder>(),
                    jsonOptions.SerializerSettings, // Just use the defaults
                    arrayPool,
                    objectPoolProvider,
                    new MvcOptions(), // The only option that NewtonsoftJsonInputFormatter uses is SuppressInputFormatterBuffering
                    jsonOptions)
            };
        }

        Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
            return BindModelAsync(bindingContext);
        }
    }

    /// <summary>
    /// A model binder for Umbraco models that require Newtonsoft Json serializers
    /// </summary>    
    public class UmbracoJsonModelBinderProvider : BodyModelBinderProvider, IModelBinderProvider
    {
        public UmbracoJsonModelBinderProvider(IHttpRequestStreamReaderFactory readerFactory, ILoggerFactory logger, ArrayPool<char> arrayPool, ObjectPoolProvider objectPoolProvider)
            : base(UmbracoJsonModelBinder.GetNewtonsoftJsonFormatter(logger, arrayPool, objectPoolProvider), readerFactory)
        {
        }

        /// <summary>
        /// Returns the model binder if it's for Umbraco models
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IModelBinder IModelBinderProvider.GetBinder(ModelBinderProviderContext context)
        {
            // Must be 'Body' (json) binding
            if (context.BindingInfo.BindingSource != BindingSource.Body)
                return null;

            if (context.Metadata?.UnderlyingOrModelType.Assembly == typeof(UmbracoJsonModelBinderProvider).Assembly // Umbraco.Web.Common
                || context.Metadata?.UnderlyingOrModelType.Assembly == typeof(IRuntimeState).Assembly // Umbraco.Core
                || context.Metadata?.UnderlyingOrModelType.Assembly == typeof(RuntimeState).Assembly) // Umbraco.Infrastructure
            {
                return GetBinder(context);
            }

            return null;
        }
    }
}
