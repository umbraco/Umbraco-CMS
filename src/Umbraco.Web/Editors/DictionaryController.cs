using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
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
    /// Dictionar
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
    [EnableOverrideAuthorization]
    public class DictionaryController : BackOfficeNotificationsController
    {
        /// <summary>
        /// Deletes a data type wth a given ID
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
            
            Services.LocalizationService.Delete(foundDictionary, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates a new dictoinairy item
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
                    .CreateNotificationValidationErrorResponse("Key can not be empty;"); // TODO translate

            if (Services.LocalizationService.DictionaryItemExists(key))
            {
                var message = Services.TextService.Localize(
                     "dictionaryItem/changeKeyError",
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
                Logger.Error(GetType(), ex, "Error creating dictionary with {Name} under {ParentId}", key, parentId);
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
        ///  Returrns a not found response when dictionary item does not exist
        /// </exception>
        public DictionaryDisplay GetById(int id)
        {
            var dictionary = Services.LocalizationService.GetDictionaryItemById(id);

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
                        "dictionaryItem/changeKeyError",
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
                    Services.TextService.Localize("speechBubbles/dictionaryItemSaved", userCulture), string.Empty,
                    SpeechBubbleIcon.Success));

                return model;
            }
            catch (Exception ex)
            {
                Logger.Error(GetType(), ex, "Error saving dictionary with {Name} under {ParentId}", dictionary.Name, dictionary.ParentId);
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
            var list = new List<DictionaryOverviewDisplay>();

            const int level = 0;

            foreach (var dictionaryItem in Services.LocalizationService.GetRootDictionaryItems().OrderBy(ItemSort()))
            {
                var item = Mapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(dictionaryItem);
                item.Level = 0;
                list.Add(item);

                GetChildItemsForList(dictionaryItem, level + 1, list);
            }

            return list;
        }

        /// <summary>
        /// Get child items for list.
        /// </summary>
        /// <param name="dictionaryItem">
        /// The dictionary item.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <param name="list">
        /// The list.
        /// </param>
        private void GetChildItemsForList(IDictionaryItem dictionaryItem, int level, ICollection<DictionaryOverviewDisplay> list)
        {
            foreach (var childItem in Services.LocalizationService.GetDictionaryItemChildren(dictionaryItem.Key).OrderBy(ItemSort()))
            {
                var item = Mapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(childItem);
                item.Level = level;
                list.Add(item);

                GetChildItemsForList(childItem, level + 1, list);
            }
        }

        private static Func<IDictionaryItem, string> ItemSort() => item => item.ItemKey;
    }
}
