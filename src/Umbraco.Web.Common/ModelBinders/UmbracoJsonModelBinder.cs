using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Web.Common.Formatters;

namespace Umbraco.Cms.Web.Common.ModelBinders;

/// <summary>
///     A custom body model binder that only uses a <see cref="NewtonsoftJsonInputFormatter" /> to bind body action
///     parameters
/// </summary>
public class UmbracoJsonModelBinder : BodyModelBinder
{
    public UmbracoJsonModelBinder(
        ArrayPool<char> arrayPool,
        ObjectPoolProvider objectPoolProvider,
        IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
        IHttpRequestStreamReaderFactory readerFactory,
        ILoggerFactory loggerFactory)
        : base(GetNewtonsoftJsonFormatter(loggerFactory, arrayPool, objectPoolProvider, jsonOptions.Value), readerFactory, loggerFactory)
    {
    }

    private static IInputFormatter[] GetNewtonsoftJsonFormatter(ILoggerFactory logger, ArrayPool<char> arrayPool, ObjectPoolProvider objectPoolProvider, MvcNewtonsoftJsonOptions jsonOptions)
    {
        jsonOptions.AllowInputFormatterExceptionMessages = true;

        JsonSerializerSettings ss = jsonOptions.SerializerSettings;

        // We need to ignore required attributes when serializing. E.g UserSave.ChangePassword. Otherwise the model is not model bound.
        ss.ContractResolver = new IgnoreRequiredAttributesResolver();
        return new IInputFormatter[]
        {
            new NewtonsoftJsonInputFormatter(
                logger.CreateLogger<UmbracoJsonModelBinder>(),
                jsonOptions.SerializerSettings, // Just use the defaults
                arrayPool,
                objectPoolProvider,
                new MvcOptions(), // The only option that NewtonsoftJsonInputFormatter uses is SuppressInputFormatterBuffering
                jsonOptions),
        };
    }
}
