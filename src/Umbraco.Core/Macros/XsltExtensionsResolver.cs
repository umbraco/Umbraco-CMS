using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Macros
{
    /// <summary>
    /// Used to resolve all xslt extension plugins
    /// </summary>
    internal sealed class XsltExtensionsResolver : LazyManyObjectsResolverBase<XsltExtensionsResolver, XsltExtension>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="packageActions"></param>		
        internal XsltExtensionsResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> packageActions)
            : base(serviceProvider, logger, packageActions, ObjectLifetimeScope.Application)
		{

		}

        /// <summary>
        /// Returns the list of all xslt extensions
        /// </summary>
        public IEnumerable<XsltExtension> XsltExtensions
		{
			get { return Values; }
		}

        protected override IEnumerable<XsltExtension> CreateInstances()
        {
            var result = new HashSet<XsltExtension>();
            foreach (var xsltType in InstanceTypes)
            {
                var tpAttributes = xsltType.GetCustomAttributes(typeof(XsltExtensionAttribute), true);
                foreach (XsltExtensionAttribute tpAttribute in tpAttributes)
                {
                    var ns = string.IsNullOrEmpty(tpAttribute.Namespace) == false
                        ? tpAttribute.Namespace
                        : xsltType.FullName;

                    result.Add(new XsltExtension(ns, Activator.CreateInstance(xsltType)));
                }
            }
            return result;
        }

        /// <summary>
        /// We override this because really there's no limit to the type that can be used here
        /// </summary>
        /// <param name="value"></param>
        protected override void EnsureCorrectType(Type value)
        {
            //do nothing.
        }

    }
}
