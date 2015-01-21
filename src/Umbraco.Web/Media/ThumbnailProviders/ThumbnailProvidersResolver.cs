using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.ObjectResolution;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;

namespace Umbraco.Web.Media.ThumbnailProviders
{
	internal sealed class ThumbnailProvidersResolver : ContainerManyObjectsResolver<ThumbnailProvidersResolver, IThumbnailProvider>
    {
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="container"></param>
	    /// <param name="logger"></param>
	    /// <param name="providers"></param>		
	    internal ThumbnailProvidersResolver(IServiceContainer container, ILogger logger, IEnumerable<Type> providers)
            : base(container, logger, providers)
		{

		}

		/// <summary>
		/// Return the providers
		/// </summary>
		public IEnumerable<IThumbnailProvider> Providers
		{
			get { return GetSortedValues(); }
		}

        public string GetThumbnailUrl(string fileUrl)
        {
            var provider = Providers.FirstOrDefault(x => x.CanProvideThumbnail(fileUrl));
            return provider != null ? provider.GetThumbnailUrl(fileUrl) : string.Empty;
        }

    }
}
