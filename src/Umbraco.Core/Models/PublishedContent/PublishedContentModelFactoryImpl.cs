using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Implements a strongly typed content model factory
    /// </summary>
    internal class PublishedContentModelFactoryImpl : IPublishedContentModelFactory
    {
        //private readonly Dictionary<string, ConstructorInfo> _constructors
        //    = new Dictionary<string, ConstructorInfo>();

        private readonly Dictionary<string, Func<IPublishedContent, IPublishedContent>> _constructors
             = new Dictionary<string, Func<IPublishedContent, IPublishedContent>>();

        public PublishedContentModelFactoryImpl()
        {
            var types = PluginManager.Current.ResolveTypes<PublishedContentModel>();
            var ctorArgTypes = new[] { typeof(IPublishedContent) };

            foreach (var type in types)
            {
                var constructor = type.GetConstructor(ctorArgTypes);
                if (constructor == null)
                    throw new InvalidOperationException(string.Format("Type {0} is missing a public constructor with one argument of type IPublishedContent.", type.FullName));
                var attribute = type.GetCustomAttribute<PublishedContentModelAttribute>(false);
                var typeName = attribute == null ? type.Name : attribute.ContentTypeAlias;
                typeName = typeName.ToLowerInvariant();

                if (_constructors.ContainsKey(typeName))
                    throw new InvalidOperationException(string.Format("More that one type want to be a model for content type {0}.", typeName));

                // should work everywhere, but slow
                //_constructors[typeName] = constructor;

                // much faster with a dynamic method but potential MediumTrust issues
                // here http://stackoverflow.com/questions/16363838/how-do-you-call-a-constructor-via-an-expression-tree-on-an-existing-object

                // fast enough and works in MediumTrust
                // read http://boxbinary.com/2011/10/how-to-run-a-unit-test-in-medium-trust-with-nunitpart-three-umbraco-framework-testing/
                var exprArg = Expression.Parameter(typeof(IPublishedContent), "content");
                var exprNew = Expression.New(constructor, exprArg);
                var expr = Expression.Lambda<Func<IPublishedContent, IPublishedContent>>(exprNew, exprArg);
                var func = expr.Compile();
                _constructors[typeName] = func;
            }
        }

        public IPublishedContent CreateModel(IPublishedContent content)
        {
            // be case-insensitive
            var contentTypeAlias = content.DocumentTypeAlias.ToLowerInvariant();

            //ConstructorInfo constructor;
            //return _constructors.TryGetValue(contentTypeAlias, out constructor)
            //    ? (IPublishedContent) constructor.Invoke(new object[] { content })
            //    : content;

            Func<IPublishedContent, IPublishedContent> constructor;
            return _constructors.TryGetValue(contentTypeAlias, out constructor)
                ? constructor(content)
                : content;
        }
    }
}
