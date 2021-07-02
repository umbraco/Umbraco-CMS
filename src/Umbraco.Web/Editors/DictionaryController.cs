using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;

namespace Umbraco.Web.Editors
{
    /// <inheritdoc />
    /// <summary>
    /// The API controller used for editing dictionary items
    /// </summary>
    /// <remarks>
    /// The security for this controller is defined to allow full CRUD access to dictionary if the user has access to either:
    /// Dictionary
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
    [EnableOverrideAuthorization]
    [DictionaryControllerConfiguration]
    public class DictionaryController : BackOfficeNotificationsController
    {
        public DictionaryController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class DictionaryControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi))
                ));
            }
        }

        /// <summary>
        /// Deletes a data type with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundDictionary = Services.LocalizationService.GetDictionaryItemById(id);

            if (foundDictionary == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var foundDictionaryDescendants = Services.LocalizationService.GetDictionaryItemDescendants(foundDictionary.Key);

            foreach (var dictionaryItem in foundDictionaryDescendants)
            {
                Services.LocalizationService.Delete(dictionaryItem, Security.CurrentUser.Id);
            }

            Services.LocalizationService.Delete(foundDictionary, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates a new dictionary item
        /// </summary>
        /// <param name="parentId">
        /// The parent id.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Create(int parentId, string key)
        {
            if (string.IsNullOrEmpty(key))
                return Request
                    .CreateNotificationValidationErrorResponse("Key can not be empty."); // TODO: translate

            if (Services.LocalizationService.DictionaryItemExists(key))
            {
                var message = Services.TextService.Localize(
                     "dictionaryItem","changeKeyError",
                     Security.CurrentUser.GetUserCulture(Services.TextService, GlobalSettings),
                     new Dictionary<string, string> { { "0", key } });
                return Request.CreateNotificationValidationErrorResponse(message);
            }

            try
            {
                Guid? parentGuid = null;

                if (parentId > 0)
                    parentGuid = Services.LocalizationService.GetDictionaryItemById(parentId).Key;

                var item = Services.LocalizationService.CreateDictionaryItemWithIdentity(
                    key,
                    parentGuid,
                    string.Empty);


                return Request.CreateResponse(HttpStatusCode.OK, item.Id);
            }
            catch (Exception ex)
            {
                Logger.Error<string,int>(GetType(), ex, "Error creating dictionary with {Name} under {ParentId}", key, parentId);
                return Request.CreateNotificationValidationErrorResponse("Error creating dictionary item");
            }
        }

        /// <summary>
        /// Gets a dictionary item by id
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="DictionaryDisplay"/>.
        /// </returns>
        /// <exception cref="HttpResponseException">
        ///  Returns a not found response when dictionary item does not exist
        /// </exception>
        public DictionaryDisplay GetById(int id)
        {
            var dictionary = Services.LocalizationService.GetDictionaryItemById(id);
            if (dictionary == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Mapper.Map<IDictionaryItem, DictionaryDisplay>(dictionary);
        }

        /// <summary>
        /// Gets a dictionary item by guid
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="DictionaryDisplay"/>.
        /// </returns>
        /// <exception cref="HttpResponseException">
        ///  Returns a not found response when dictionary item does not exist
        /// </exception>
        public DictionaryDisplay GetById(Guid id)
        {
            var dictionary = Services.LocalizationService.GetDictionaryItemById(id);
            if (dictionary == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Mapper.Map<IDictionaryItem, DictionaryDisplay>(dictionary);
        }

        /// <summary>
        /// Gets a dictionary item by udi
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="DictionaryDisplay"/>.
        /// </returns>
        /// <exception cref="HttpResponseException">
        ///  Returns a not found response when dictionary item does not exist
        /// </exception>
        public DictionaryDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var dictionary = Services.LocalizationService.GetDictionaryItemById(guidUdi.Guid);
            if (dictionary == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Mapper.Map<IDictionaryItem, DictionaryDisplay>(dictionary);
        }

        /// <summary>
        /// Saves a dictionary item
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary.
        /// </param>
        /// <returns>
        /// The <see cref="DictionaryDisplay"/>.
        /// </returns>
        public DictionaryDisplay PostSave(DictionarySave dictionary)
        {
            var dictionaryItem =
                Services.LocalizationService.GetDictionaryItemById(int.Parse(dictionary.Id.ToString()));

            if (dictionaryItem == null)
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Dictionary item does not exist"));

            var userCulture = Security.CurrentUser.GetUserCulture(Services.TextService, GlobalSettings);

            if (dictionary.NameIsDirty)
            {
                // if the name (key) has changed, we need to check if the new key does not exist
                var dictionaryByKey = Services.LocalizationService.GetDictionaryItemByKey(dictionary.Name);

                if (dictionaryByKey != null && dictionaryItem.Id != dictionaryByKey.Id)
                {

                    var message = Services.TextService.Localize(
                        "dictionaryItem","changeKeyError",
                        userCulture,
                        new Dictionary<string, string> { { "0", dictionary.Name } });
                    ModelState.AddModelError("Name", message);
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
                }

                dictionaryItem.ItemKey = dictionary.Name;
            }

            foreach (var translation in dictionary.Translations)
            {
                Services.LocalizationService.AddOrUpdateDictionaryValue(dictionaryItem,
                    Services.LocalizationService.GetLanguageById(translation.LanguageId), translation.Translation);
            }

            try
            {
                Services.LocalizationService.Save(dictionaryItem);

                var model = Mapper.Map<IDictionaryItem, DictionaryDisplay>(dictionaryItem);

                model.Notifications.Add(new Notification(
                    Services.TextService.Localize("speechBubbles","dictionaryItemSaved", userCulture), string.Empty,
                    NotificationStyle.Success));

                return model;
            }
            catch (Exception ex)
            {
                Logger.Error<string,Guid>(GetType(), ex, "Error saving dictionary with {Name} under {ParentId}", dictionary.Name, dictionary.ParentId);
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Something went wrong saving dictionary"));
            }
        }

        /// <summary>
        /// Retrieves a list with all dictionary items
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        public IEnumerable<DictionaryOverviewDisplay> GetList()
        {
            var items = Services.LocalizationService.GetDictionaryItemDescendants(null).ToArray();
            var list = new List<DictionaryOverviewDisplay>(items.Length);

            // recursive method to build a tree structure from the flat structure returned above
            void BuildTree(int level = 0, Guid? parentId = null)
            {
                var children = items.Where(t => t.ParentId == parentId).ToArray();
                if(children.Any() == false)
                {
                    return;
                }

                foreach(var child in children.OrderBy(ItemSort()))
                {
                    var display = Mapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(child);
                    display.Level = level;
                    list.Add(display);

                    BuildTree(level + 1, child.Key);
                }                
            }

            BuildTree();

            return list;            
        }

        private static Func<IDictionaryItem, string> ItemSort() => item => item.ItemKey;
    }
}
