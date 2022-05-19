using System.Collections;
using System.Reflection;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Implements a strongly typed content model factory
/// </summary>
public class PublishedModelFactory : IPublishedModelFactory
{
    private readonly Dictionary<string, ModelInfo>? _modelInfos;
    private readonly Dictionary<string, Type> _modelTypeMap;
    private readonly IPublishedValueFallback _publishedValueFallback;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedModelFactory" /> class with types.
    /// </summary>
    /// <param name="types">The model types.</param>
    /// <param name="publishedValueFallback"></param>
    /// <remarks>
    ///     <para>
    ///         Types must implement <c>IPublishedContent</c> and have a unique constructor that
    ///         accepts one IPublishedContent as a parameter.
    ///     </para>
    ///     <para>To activate,</para>
    ///     <code>
    /// var types = TypeLoader.Current.GetTypes{PublishedContentModel}();
    /// var factory = new PublishedContentModelFactoryImpl(types);
    /// PublishedContentModelFactoryResolver.Current.SetFactory(factory);
    /// </code>
    /// </remarks>
    public PublishedModelFactory(IEnumerable<Type> types, IPublishedValueFallback publishedValueFallback)
    {
        var modelInfos = new Dictionary<string, ModelInfo>(StringComparer.InvariantCultureIgnoreCase);
        var modelTypeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        foreach (Type type in types)
        {
            // so... the model type has to implement a ctor with one parameter being, or inheriting from,
            // IPublishedElement - but it can be IPublishedContent - so we cannot get one precise ctor,
            // we have to iterate over all ctors and try to find the right one
            ConstructorInfo? constructor = null;
            Type? parameterType = null;

            foreach (ConstructorInfo ctor in type.GetConstructors())
            {
                ParameterInfo[] parms = ctor.GetParameters();
                if (parms.Length == 2 && typeof(IPublishedElement).IsAssignableFrom(parms[0].ParameterType) &&
                    typeof(IPublishedValueFallback).IsAssignableFrom(parms[1].ParameterType))
                {
                    if (constructor != null)
                    {
                        throw new InvalidOperationException(
                            $"Type {type.FullName} has more than one public constructor with one argument of type, or implementing, IPublishedElement.");
                    }

                    constructor = ctor;
                    parameterType = parms[0].ParameterType;
                }
            }

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    $"Type {type.FullName} is missing a public constructor with one argument of type, or implementing, IPublishedElement.");
            }

            PublishedModelAttribute? attribute = type.GetCustomAttribute<PublishedModelAttribute>(false);
            var typeName = attribute == null ? type.Name : attribute.ContentTypeAlias;

            if (modelInfos.TryGetValue(typeName, out ModelInfo? modelInfo))
            {
                throw new InvalidOperationException(
                    $"Both types '{type.AssemblyQualifiedName}' and '{modelInfo.ModelType?.AssemblyQualifiedName}' want to be a model type for content type with alias \"{typeName}\".");
            }

            // have to use an unsafe ctor because we don't know the types, really
            Func<object, IPublishedValueFallback, object> modelCtor =
                ReflectionUtilities.EmitConstructorUnsafe<Func<object, IPublishedValueFallback, object>>(constructor);
            modelInfos[typeName] = new ModelInfo { ParameterType = parameterType, ModelType = type, Ctor = modelCtor };
            modelTypeMap[typeName] = type;
        }

        _modelInfos = modelInfos.Count > 0 ? modelInfos : null;
        _modelTypeMap = modelTypeMap;
        _publishedValueFallback = publishedValueFallback;
    }

    /// <inheritdoc />
    public IPublishedElement CreateModel(IPublishedElement element)
    {
        // fail fast
        if (_modelInfos is null || element.ContentType.Alias is null ||
            !_modelInfos.TryGetValue(element.ContentType.Alias, out ModelInfo? modelInfo))
        {
            return element;
        }

        // ReSharper disable once UseMethodIsInstanceOfType
        if (modelInfo.ParameterType?.IsAssignableFrom(element.GetType()) == false)
        {
            throw new InvalidOperationException(
                $"Model {modelInfo.ModelType} expects argument of type {modelInfo.ParameterType.FullName}, but got {element.GetType().FullName}.");
        }

        // can cast, because we checked when creating the ctor
        return (IPublishedElement)modelInfo.Ctor!(element, _publishedValueFallback);
    }

    /// <inheritdoc />
    public IList? CreateModelList(string? alias)
    {
        // fail fast
        if (_modelInfos is null || alias is null || !_modelInfos.TryGetValue(alias, out ModelInfo? modelInfo) ||
            modelInfo.ModelType is null)
        {
            return new List<IPublishedElement>();
        }

        Func<IList>? ctor = modelInfo.ListCtor;
        if (ctor != null)
        {
            return ctor();
        }

        Type listType = typeof(List<>).MakeGenericType(modelInfo.ModelType);
        ctor = modelInfo.ListCtor = ReflectionUtilities.EmitConstructor<Func<IList>>(declaring: listType);
        if (ctor is not null)
        {
            return ctor();
        }

        return null;
    }

    /// <inheritdoc />
    public Type GetModelType(string? alias)
    {
        // fail fast
        if (_modelInfos is null ||
            alias is null ||
            !_modelInfos.TryGetValue(alias, out ModelInfo? modelInfo) || modelInfo.ModelType is null)
        {
            return typeof(IPublishedElement);
        }

        return modelInfo.ModelType;
    }

    /// <inheritdoc />
    public Type MapModelType(Type type)
        => ModelType.Map(type, _modelTypeMap);

    private class ModelInfo
    {
        public Type? ParameterType { get; set; }

        public Func<object, IPublishedValueFallback, object>? Ctor { get; set; }

        public Type? ModelType { get; set; }

        public Func<IList>? ListCtor { get; set; }
    }
}
