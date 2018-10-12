using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi.Binders
{
    internal class BlueprintItemBinder : ContentItemBinder
    {
        public BlueprintItemBinder(ApplicationContext applicationContext)
            : base(applicationContext)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BlueprintItemBinder()
            : this(ApplicationContext.Current)
        {
        }

        protected override IContent GetExisting(ContentItemSave model)
        {
            return ApplicationContext.Services.ContentService.GetBlueprintById(Convert.ToInt32(model.Id));
        }        
    }
}