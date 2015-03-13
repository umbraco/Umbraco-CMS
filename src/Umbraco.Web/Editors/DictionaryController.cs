using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Umbraco.Web.WebApi;
using umbraco;
using System.Collections.Generic;

namespace Umbraco.Web.Editors
{
	/// <summary>
	/// The API controller used for editing dictionary items
	/// </summary>
	[PluginController("UmbracoApi")]
	[UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
	public class DictionaryController : UmbracoAuthorizedJsonController
	{
		[HttpDelete]
		[HttpPost]
		public HttpResponseMessage DeleteById(int id)
		{
			var foundType = Services.LocalizationService.GetDictionaryItemById(id);

			if (foundType == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}

			Services.LocalizationService.Delete(foundType, Security.CurrentUser.Id);

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		public IEnumerable<DictionaryItemDisplay> GetAll()
		{
			var dictionaryItems = Services.LocalizationService.GetRootDictionaryItems();

			if (dictionaryItems == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}

			return dictionaryItems
				.Select(x => Mapper.Map<IDictionaryItem, DictionaryItemDisplay>(x));
		}

		public DictionaryItemDisplay GetById(int id)
		{
			var dictionaryItem = Services.LocalizationService.GetDictionaryItemById(id);

			if (dictionaryItem == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}

			return Mapper.Map<IDictionaryItem, DictionaryItemDisplay>(dictionaryItem);
		}

		public DictionaryItemDisplay GetEmpty()
		{
			var item = new DictionaryItem("");
			return Mapper.Map<IDictionaryItem, DictionaryItemDisplay>(item);
		}

		public DictionaryItemDisplay PostSave(DictionaryItemSave dictionaryItem)
		{
			var currentValue = Services.LocalizationService.GetDictionaryItemById(dictionaryItem.PersistedDictionaryItem.Id);

			try
			{
				Services.LocalizationService.Save(currentValue, Security.CurrentUser.Id);
			}
			catch (DuplicateNameException ex)
			{
				ModelState.AddModelError("Name", ex.Message);
				throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
			}

			var display = Mapper.Map<IDictionaryItem, DictionaryItemDisplay>(dictionaryItem.PersistedDictionaryItem);

			display.AddSuccessNotification(ui.Text("speechBubbles", "dictionaryItemSaved"), "");

			return display;
		}
	}
}