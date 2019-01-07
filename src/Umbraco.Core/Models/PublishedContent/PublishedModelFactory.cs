using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Implements a strongly typed content model factory
    /// </summary>
    public class PublishedModelFactory : IPublishedModelFactory
    {
        private readonly Dictionary<string, ModelInfo> _modelInfos;
        private readonly Dictionary<string, Type> _modelTypeMap;

        private class ModelInfo
        {
            public Type ParameterType { get; set; }
            public Func<object, object> Ctor { get; set; }
            public Type ModelType { get; set; }
            public Func<IList> ListCtor { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedModelFactory"/> class with types.
        /// </summary>
        /// <param name="types">The model types.</param>
        /// <remarks>
        /// <para>Types must implement <c>IPublishedContent</c> and have a unique constructor that
        /// accepts one IPublishedContent as a parameter.</para>
        /// <para>To activate,</para>
        /// <code>
        /// var types = TypeLoader.Current.GetTypes{PublishedContentModel}();
        /// var factory = new PublishedContentModelFactoryImpl(types);
        /// PublishedContentModelFactoryResolver.Current.SetFactory(factory);
        /// </code>
        /// </remarks>
        public PublishedModelFactory(IEnumerable<Type> types)
        {
            var modelInfos = new Dictionary<string, ModelInfo>(StringComparer.InvariantCultureIgnoreCase);
            var modelTypeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var type in types)
            {
                // so... the model type has to implement a ctor with one parameter being, or inheriting from,
                // IPublishedElement - but it can be IPublishedContent - so we cannot get one precise ctor,
                // we have to iterate over all ctors and try to find the right one

                ConstructorInfo constructor = null;
                Type parameterType = null;

                foreach (var ctor in type.GetConstructors())
                {
                    var parms = ctor.GetParameters();
                    if (parms.Length == 1 && typeof(IPublishedElement).IsAssignableFrom(parms[0].ParameterType))
                    {
                        if (constructor != null)
                            throw new InvalidOperationException($"Type {type.FullName} has more than one public constructor with one argument of type, or implementing, IPublishedElement.");
                        constructor = ctor;
                        parameterType = parms[0].ParameterType;
                    }
                }

                if (constructor == null)
                    throw new InvalidOperationException($"Type {type.FullName} is missing a public constructor with one argument of type, or implementing, IPublishedElement.");

                var attribute = type.GetCustomAttribute<PublishedModelAttribute>(false);
                var typeName = attribute == null ? type.Name : attribute.ContentTypeAlias;

                if (modelInfos.TryGetValue(typeName, out var modelInfo))
                    throw new InvalidOperationException($"Both types {type.FullName} and {modelInfo.ModelType.FullName} want to be a model type for content type with alias \"{typeName}\".");

                // have to use an unsafe ctor because we don't know the types, really
                var modelCtor = ReflectionUtilities.EmitConstructorUnsafe<Func<object, object>>(constructor);
                modelInfos[typeName] = new ModelInfo { ParameterType = parameterType, ModelType = type, Ctor = modelCtor };
                modelTypeMap[typeName] = type;
            }

            _modelInfos = modelInfos.Count > 0 ? modelInfos : null;
            _modelTypeMap = modelTypeMap;
        }

        /// <inheritdoc />
        public IPublishedElement CreateModel(IPublishedElement element)
        {
            // fail fast
            if (_modelInfos == null)
                return element;

            if (!_modelInfos.TryGetValue(element.ContentType.Alias, out var modelInfo))
                return element;

            // ReSharper disable once UseMethodIsInstanceOfType
            if (modelInfo.ParameterType.IsAssignableFrom(element.GetType()) == false)
                throw new InvalidOperationException($"Model {modelInfo.ModelType} expects argument of type {modelInfo.ParameterType.FullName}, but got {element.GetType().FullName}.");

            // can cast, because we checked when creating the ctor
            return (IPublishedElement) modelInfo.Ctor(element);
        }

        /// <inheritdoc />
        public IList CreateModelList(string alias)
        {
            // fail fast
            if (_modelInfos == null)
                return new List<IPublishedElement>();

            if (!_modelInfos.TryGetValue(alias, out var modelInfo))
                return new List<IPublishedElement>();

            var ctor = modelInfo.ListCtor;
            if (ctor != null) return ctor();

            var listType = typeof(List<>).MakeGenericType(modelInfo.ModelType);
            ctor = modelInfo.ListCtor = ReflectionUtilities.EmitConstructor<Func<IList>>(declaring: listType);
            return ctor();
        }

        /// <inheritdoc />
        public Type MapModelType(Type type)
            => ModelType.Map(type, _modelTypeMap);
    }
}
