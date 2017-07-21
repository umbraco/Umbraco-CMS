using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Implements a strongly typed content model factory
    /// </summary>
    public class PublishedContentModelFactory : IPublishedContentModelFactory
    {
        private readonly Dictionary<string, ModelInfo> _modelInfos;

        private class ModelInfo
        {
            public Type ParameterType { get; set; }
            public Func<IPropertySet, IPropertySet> Ctor { get; set; }
            public Type ModelType { get; set; }
        }

        public Dictionary<string, Type> ModelTypeMap { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentModelFactory"/> class with types.
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
        public PublishedContentModelFactory(IEnumerable<Type> types)
        {
            var ctorArgTypes = new[] { typeof(IPropertySet) };
            var modelInfos = new Dictionary<string, ModelInfo>(StringComparer.InvariantCultureIgnoreCase);

            ModelTypeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var type in types)
            {
                ConstructorInfo constructor = null;
                Type parameterType = null;

                foreach (var ctor in type.GetConstructors())
                {
                    var parms = ctor.GetParameters();
                    if (parms.Length == 1 && typeof (IPropertySet).IsAssignableFrom(parms[0].ParameterType))
                    {
                        if (constructor != null)
                            throw new InvalidOperationException($"Type {type.FullName} has more than one public constructor with one argument of type, or implementing, IPropertySet.");
                        constructor = ctor;
                        parameterType = parms[0].ParameterType;
                    }
                }

                if (constructor == null)
                    throw new InvalidOperationException($"Type {type.FullName} is missing a public constructor with one argument of type, or implementing, IPropertySet.");

                var attribute = type.GetCustomAttribute<PublishedContentModelAttribute>(false); // fixme rename FacadeModelAttribute
                var typeName = attribute == null ? type.Name : attribute.ContentTypeAlias;

                if (modelInfos.TryGetValue(typeName, out ModelInfo modelInfo))
                    throw new InvalidOperationException($"Both types {type.FullName} and {modelInfo.ModelType.FullName} want to be a model type for content type with alias \"{typeName}\".");

                // see Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks
                // using ctor.Invoke is horrible, cannot even consider it,
                // then expressions are 6-10x slower than direct ctor, and
                // dynamic methods are 2-3x slower than direct ctor = best

                // much faster with a dynamic method but potential MediumTrust issues - which we don't support
                // here http://stackoverflow.com/questions/16363838/how-do-you-call-a-constructor-via-an-expression-tree-on-an-existing-object
                var meth = new DynamicMethod(string.Empty, typeof(IPropertySet), ctorArgTypes, type.Module, true);
                var gen = meth.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Newobj, constructor);
                gen.Emit(OpCodes.Ret);
                var func = (Func<IPropertySet, IPropertySet>) meth.CreateDelegate(typeof (Func<IPropertySet, IPropertySet>));

                // fast enough and works in MediumTrust - but we don't
                // read http://boxbinary.com/2011/10/how-to-run-a-unit-test-in-medium-trust-with-nunitpart-three-umbraco-framework-testing/
                //var exprArg = Expression.Parameter(typeof(IPropertySet), "content");
                //var exprNew = Expression.New(constructor, exprArg);
                //var expr = Expression.Lambda<Func<IPropertySet, IPropertySet>>(exprNew, exprArg);
                //var func = expr.Compile();

                modelInfos[typeName] = new ModelInfo { ParameterType = parameterType, Ctor = func, ModelType = type };
                ModelTypeMap[typeName] = type;
            }

            _modelInfos = modelInfos.Count > 0 ? modelInfos : null;
        }

        public IPropertySet CreateModel(IPropertySet set)
        {
            // fail fast
            if (_modelInfos == null)
                return set;

            if (_modelInfos.TryGetValue(set.ContentType.Alias, out ModelInfo modelInfo) == false)
                return set;

            // ReSharper disable once UseMethodIsInstanceOfType
            if (modelInfo.ParameterType.IsAssignableFrom(set.GetType()) == false)
                throw new InvalidOperationException($"Model {modelInfo.ModelType} expects argument of type {modelInfo.ParameterType.FullName}, but got {set.GetType().FullName}.");

            return modelInfo.Ctor(set);
        }
    }
}
