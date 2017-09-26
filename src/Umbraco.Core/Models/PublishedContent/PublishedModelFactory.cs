using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Implements a strongly typed content model factory
    /// </summary>
    public class PublishedModelFactory : IPublishedModelFactory
    {
        private readonly Dictionary<string, ModelInfo> _modelInfos;

        private class ModelInfo
        {
            public Type ParameterType { get; set; }
            public Func<IPublishedElement, IPublishedElement> Ctor { get; set; }
            public Type ModelType { get; set; }
        }

        public Dictionary<string, Type> ModelTypeMap { get; }

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
            var exprs = new List<Expression<Func<IPublishedElement, IPublishedElement>>>();

            ModelTypeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var type in types)
            {
                ConstructorInfo constructor = null;
                Type parameterType = null;

                foreach (var ctor in type.GetConstructors())
                {
                    var parms = ctor.GetParameters();
                    if (parms.Length == 1 && typeof (IPublishedElement).IsAssignableFrom(parms[0].ParameterType))
                    {
                        if (constructor != null)
                            throw new InvalidOperationException($"Type {type.FullName} has more than one public constructor with one argument of type, or implementing, IPublishedElement.");
                        constructor = ctor;
                        parameterType = parms[0].ParameterType;
                    }
                }

                if (constructor == null)
                    throw new InvalidOperationException($"Type {type.FullName} is missing a public constructor with one argument of type, or implementing, IPublishedElement.");

                var attribute = type.GetCustomAttribute<PublishedContentModelAttribute>(false); // fixme rename FacadeModelAttribute
                var typeName = attribute == null ? type.Name : attribute.ContentTypeAlias;

                if (modelInfos.TryGetValue(typeName, out ModelInfo modelInfo))
                    throw new InvalidOperationException($"Both types {type.FullName} and {modelInfo.ModelType.FullName} want to be a model type for content type with alias \"{typeName}\".");

                exprs.Add(Expression.Lambda<Func<IPublishedElement, IPublishedElement>>(Expression.New(constructor)));
                modelInfos[typeName] = new ModelInfo { ParameterType = parameterType, ModelType = type };
                ModelTypeMap[typeName] = type;
            }

            var compiled = ReflectionUtilities.CompileToDelegates(exprs.ToArray());
            var i = 0;
            foreach (var modelInfo in modelInfos.Values)
                modelInfo.Ctor = compiled[i++];

            _modelInfos = modelInfos.Count > 0 ? modelInfos : null;
        }

        public IPublishedElement CreateModel(IPublishedElement element)
        {
            // fail fast
            if (_modelInfos == null)
                return element;

            if (_modelInfos.TryGetValue(element.ContentType.Alias, out var modelInfo) == false)
                return element;

            // ReSharper disable once UseMethodIsInstanceOfType
            if (modelInfo.ParameterType.IsAssignableFrom(element.GetType()) == false)
                throw new InvalidOperationException($"Model {modelInfo.ModelType} expects argument of type {modelInfo.ParameterType.FullName}, but got {element.GetType().FullName}.");

            return modelInfo.Ctor(element);
        }
    }
}
