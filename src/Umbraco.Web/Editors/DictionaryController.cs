namespace Umbraco.Web.Editors
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
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
            var foundDictionary = this.Services.LocalizationService.GetDictionaryItemById(id);
            if (foundDictionary == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            this.Services.LocalizationService.Delete(foundDictionary, Security.CurrentUser.Id);

            return this.Request.CreateResponse(HttpStatusCode.OK);
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
            {

                return this.Request
                    .CreateNotificationValidationErrorResponse("Key can not be empty;"); // TODO translate
            }

            if (this.Services.LocalizationService.DictionaryItemExists(key))
            {
                var message = this.Services.TextService.Localize(
                     "dictionaryItem/changeKeyError",
                     this.Security.CurrentUser.GetUserCulture(this.Services.TextService),
                     new Dictionary<string, string> { { "0", key } });
                return this.Request.CreateNotificationValidationErrorResponse(message);
            }

            try
            {
                Guid? parentGuid = null;

                if (parentId > 0)
                {
                    parentGuid = this.Services.LocalizationService.GetDictionaryItemById(parentId).Key;
                }

                var item = this.Services.LocalizationService.CreateDictionaryItemWithIdentity(
                    key,
                    parentGuid,
                    string.Empty);


                return this.Request.CreateResponse(HttpStatusCode.OK, item.Id);
            }
            catch (Exception exception)
            {
                this.Logger.Error(this.GetType(), "Error creating dictionary", exception);
                return this.Request.CreateNotificationValidationErrorResponse("Error creating dictionary item");
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
            var dictionary = this.Services.LocalizationService.GetDictionaryItemById(id);

            if (dictionary == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

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
                this.Services.LocalizationService.GetDictionaryItemById(int.Parse(dictionary.Id.ToString()));

            if (dictionaryItem != null)
            {
                var userCulture = this.Security.CurrentUser.GetUserCulture(this.Services.TextService);

                if (dictionary.NameIsDirty)
                {
                    // if the name (key) has changed, we need to check if the new key does not exist
                    var dictionaryByKey = this.Services.LocalizationService.GetDictionaryItemByKey(dictionary.Name);

                    if (dictionaryByKey != null && dictionaryItem.Id != dictionaryByKey.Id)
                    {

                        var message = this.Services.TextService.Localize(
                            "dictionaryItem/changeKeyError",
                            userCulture,
                            new Dictionary<string, string> { { "0", dictionary.Name } });
                        this.ModelState.AddModelError("Name", message);
                        throw new HttpResponseException(this.Request.CreateValidationErrorResponse(ModelState));
                    }

                    dictionaryItem.ItemKey = dictionary.Name;
                }

                foreach (var translation in dictionary.Translations)
                {
                    this.Services.LocalizationService.AddOrUpdateDictionaryValue(dictionaryItem, this.Services.LocalizationService.GetLanguageById(translation.LanguageId), translation.Translation);
                }

                try
                {
                    this.Services.LocalizationService.Save(dictionaryItem);

                    var model = Mapper.Map<IDictionaryItem, DictionaryDisplay>(dictionaryItem);

                    model.Notifications.Add(new Models.ContentEditing.Notification(this.Services.TextService.Localize("speechBubbles/dictionaryItemSaved", userCulture), string.Empty, SpeechBubbleIcon.Success));

                    return model;
                }
                catch (Exception e)
                {
                    this.Logger.Error(this.GetType(), "Error saving dictionary", e);
                    throw new HttpResponseException(this.Request.CreateNotificationValidationErrorResponse("Something went wrong saving dictionary"));
                }
            }

            throw new HttpResponseException(this.Request.CreateNotificationValidationErrorResponse("Dictionary item does not exist"));
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

            var level = 0;

            foreach (var dictionaryItem in this.Services.LocalizationService.GetRootDictionaryItems())
            {
                var item = Mapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(dictionaryItem);
                item.Level = 0;
                list.Add(item);

                this.GetChildItemsForList(dictionaryItem, level + 1, list);
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
        private void GetChildItemsForList(IDictionaryItem dictionaryItem, int level, List<DictionaryOverviewDisplay> list)
        {
            foreach (var childItem in this.Services.LocalizationService.GetDictionaryItemChildren(
                dictionaryItem.Key))
            {
                var item = Mapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(childItem);
                item.Level = level;
                list.Add(item);

                this.GetChildItemsForList(childItem, level + 1, list);
            }
        }
    }
}
