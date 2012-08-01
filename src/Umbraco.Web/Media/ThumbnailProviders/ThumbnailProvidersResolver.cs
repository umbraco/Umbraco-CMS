using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Interfaces;
using Umbraco.Core.Resolving;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;

namespace Umbraco.Web.Media.ThumbnailProviders
{
	internal sealed class ThumbnailProvidersResolver : ManyObjectsResolverBase<IThumbnailProvider>
    {

		#region Singleton

		private static readonly ThumbnailProvidersResolver Instance = 
			new ThumbnailProvidersResolver(PluginTypeResolver.Current.ResolveThumbnailProviders());

		public static ThumbnailProvidersResolver Current
		{
			get { return Instance; }
		}
		#endregion

		#region Constructors
		static ThumbnailProvidersResolver() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="providers"></param>		
		internal ThumbnailProvidersResolver(IEnumerable<Type> providers)
			: base(providers)
		{

		}
		#endregion

		public IEnumerable<IThumbnailProvider> Providers
		{
			get 
			{ 				
				var vals = Values.ToList();
				//ensure they are sorted
				vals.Sort((f1, f2) => f1.Priority.CompareTo(f2.Priority));
				return vals;
			}
		}

        public string GetThumbnailUrl(string fileUrl)
        {
            var provider = Providers.FirstOrDefault(x => x.CanProvideThumbnail(fileUrl));
            return provider != null ? provider.GetThumbnailUrl(fileUrl) : string.Empty;
        }

    }
}
