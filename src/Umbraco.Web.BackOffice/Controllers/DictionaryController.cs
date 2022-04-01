using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// The API controller used for editing dictionary items
    /// </summary>
    /// <remarks>
    /// The security for this controller is defined to allow full CRUD access to dictionary if the user has access to either:
    /// Dictionary
    /// </remarks>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDictionary)]
    [ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
    public class DictionaryController : BackOfficeNotificationsController
    {
        private readonly ILogger<DictionaryController> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly GlobalSettings _globalSettings;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUmbracoMapper _umbracoMapper;

        public DictionaryController(
            ILogger<DictionaryController> logger,
            ILocalizationService localizationService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IOptions<GlobalSettings> globalSettings,
            ILocalizedTextService localizedTextService,
            IUmbracoMapper umbracoMapper
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
            _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        }

        /// <summary>
        /// Deletes a data type with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteById(int id)
        {
            var foundDictionary = _localizationService.GetDictionaryItemById(id);

            if (foundDictionary == null)
                return NotFound();

            var foundDictionaryDescendants = _localizationService.GetDictionaryItemDescendants(foundDictionary.Key);

            foreach (var dictionaryItem in foundDictionaryDescendants)
            {
                _localizationService.Delete(dictionaryItem, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
            }

            _localizationService.Delete(foundDictionary, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

            return Ok();
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
        public ActionResult<int> Create(int parentId, string key)
        {
            if (string.IsNullOrEmpty(key))
                return ValidationProblem("Key can not be empty."); // TODO: translate

            if (_localizationService.DictionaryItemExists(key))
            {
                var message = _localizedTextService.Localize(
                     "dictionaryItem","changeKeyError",
                     _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.GetUserCulture(_localizedTextService, _globalSettings),
                     new Dictionary<string, string> { { "0", key } });
                return ValidationProblem(message);
            }

            try
            {
                Guid? parentGuid = null;

                if (parentId > 0)
                    parentGuid = _localizationService.GetDictionaryItemById(parentId).Key;

                var item = _localizationService.CreateDictionaryItemWithIdentity(
                    key,
                    parentGuid,
                    string.Empty);


                return item.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dictionary with {Name} under {ParentId}", key, parentId);
                return ValidationProblem("Error creating dictionary item");
            }
        }

     /// <summary>
        /// Gets a dictionary item by id
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="DictionaryDisplay"/>. Returns a not found response when dictionary item does not exist
        /// </returns>
        public ActionResult<DictionaryDisplay> GetById(int id)
        {
            var dictionary = _localizationService.GetDictionaryItemById(id);
            if (dictionary == null)
                return NotFound();

            return _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionary);
        }

        /// <summary>
        /// Gets a dictionary item by guid
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="DictionaryDisplay"/>. Returns a not found response when dictionary item does not exist
        /// </returns>
        public ActionResult<DictionaryDisplay> GetById(Guid id)
        {
            var dictionary = _localizationService.GetDictionaryItemById(id);
            if (dictionary == null)
                return NotFound();

            return _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionary);
        }

        /// <summary>
        /// Gets a dictionary item by udi
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="DictionaryDisplay"/>. Returns a not found response when dictionary item does not exist
        /// </returns>
        public ActionResult<DictionaryDisplay> GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                return NotFound();

            var dictionary = _localizationService.GetDictionaryItemById(guidUdi.Guid);
            if (dictionary == null)
                return NotFound();

            return _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionary);
        }

        /// <summary>
        /// Changes the structure for dictionary items
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public IActionResult PostMove(MoveOrCopy move)
        {
            var dictionaryItem = _localizationService.GetDictionaryItemById(move.Id);
            if (dictionaryItem == null)
                return ValidationProblem(_localizedTextService.Localize("dictionary", "itemDoesNotExists"));

            var parent = _localizationService.GetDictionaryItemById(move.ParentId);
            if (parent == null)
            {
                if (move.ParentId == Constants.System.Root)
                    return ValidationProblem(_localizedTextService.Localize("dictionary", "parentDoesNotExists"));
                else
                    dictionaryItem.ParentId = null;
            }
            else
            {
                dictionaryItem.ParentId = parent.Key;
                if (dictionaryItem.Key == parent.ParentId)              
                    return ValidationProblem(_localizedTextService.Localize("moveOrCopy", "notAllowedByPath"));               
            }
               
            _localizationService.Save(dictionaryItem);

            var model = _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionaryItem);

            return Content(model.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
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
        public ActionResult<DictionaryDisplay> PostSave(DictionarySave dictionary)
        {
            var dictionaryItem =
                _localizationService.GetDictionaryItemById(int.Parse(dictionary.Id.ToString(), CultureInfo.InvariantCulture));

            if (dictionaryItem == null)
                return ValidationProblem("Dictionary item does not exist");

            var userCulture = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.GetUserCulture(_localizedTextService, _globalSettings);

            if (dictionary.NameIsDirty)
            {
                // if the name (key) has changed, we need to check if the new key does not exist
                var dictionaryByKey = _localizationService.GetDictionaryItemByKey(dictionary.Name);

                if (dictionaryByKey != null && dictionaryItem.Id != dictionaryByKey.Id)
                {

                    var message = _localizedTextService.Localize(
                        "dictionaryItem","changeKeyError",
                        userCulture,
                        new Dictionary<string, string> { { "0", dictionary.Name } });
                    ModelState.AddModelError("Name", message);
                    return ValidationProblem(ModelState);
                }

                dictionaryItem.ItemKey = dictionary.Name;
            }

            foreach (var translation in dictionary.Translations)
            {
                _localizationService.AddOrUpdateDictionaryValue(dictionaryItem,
                    _localizationService.GetLanguageById(translation.LanguageId), translation.Translation);
            }

            try
            {
                _localizationService.Save(dictionaryItem);

                var model = _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionaryItem);

                model.Notifications.Add(new BackOfficeNotification(
                    _localizedTextService.Localize("speechBubbles","dictionaryItemSaved", userCulture), string.Empty,
                    NotificationStyle.Success));

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving dictionary with {Name} under {ParentId}", dictionary.Name, dictionary.ParentId);
                return ValidationProblem("Something went wrong saving dictionary");
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
            var items = _localizationService.GetDictionaryItemDescendants(null).ToArray();
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
                    var display = _umbracoMapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(child);
                    display.Level = level;
                    list.Add(display);

                    BuildTree(level + 1, child.Key);
                }
            }

            BuildTree();

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
            foreach (var childItem in _localizationService.GetDictionaryItemChildren(dictionaryItem.Key).OrderBy(ItemSort()))
            {
                var item = _umbracoMapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(childItem);
                item.Level = level;
                list.Add(item);

                GetChildItemsForList(childItem, level + 1, list);
            }
        }

        private static Func<IDictionaryItem, string> ItemSort() => item => item.ItemKey;
    }
}
