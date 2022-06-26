using System;
using System.Xml;
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
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Umbraco.Cms.Infrastructure.Packaging;
using Constants = Umbraco.Cms.Core.Constants;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

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
        private readonly IEntityXmlSerializer _serializer;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly PackageDataInstallation _packageDataInstallation;

        public DictionaryController(
            ILogger<DictionaryController> logger,
            ILocalizationService localizationService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IOptionsSnapshot<GlobalSettings> globalSettings,
            ILocalizedTextService localizedTextService,
            IUmbracoMapper umbracoMapper,
            IEntityXmlSerializer serializer,
            IHostingEnvironment hostingEnvironment,
            PackageDataInstallation packageDataInstallation
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
            _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _packageDataInstallation = packageDataInstallation ?? throw new ArgumentNullException(nameof(packageDataInstallation));
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
                _localizationService.Delete(dictionaryItem, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);
            }

            _localizationService.Delete(foundDictionary, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

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
                     _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.GetUserCulture(_localizedTextService, _globalSettings),
                     new Dictionary<string, string?> { { "0", key } });
                return ValidationProblem(message);
            }

            try
            {
                Guid? parentGuid = null;

                if (parentId > 0)
                    parentGuid = _localizationService.GetDictionaryItemById(parentId)?.Key;

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
        public ActionResult<DictionaryDisplay?> GetById(int id)
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
        public ActionResult<DictionaryDisplay?> GetById(Guid id)
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
        public ActionResult<DictionaryDisplay?> GetById(Udi id)
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
        public IActionResult? PostMove(MoveOrCopy move)
        {
            var dictionaryItem = _localizationService.GetDictionaryItemById(move.Id);
            if (dictionaryItem == null)
                return ValidationProblem(_localizedTextService.Localize("dictionary", "itemDoesNotExists"));

            var parent = _localizationService.GetDictionaryItemById(move.ParentId);
            if (parent == null)
            {
                if (move.ParentId == Constants.System.Root)
                    dictionaryItem.ParentId = null;
                else
                    return ValidationProblem(_localizedTextService.Localize("dictionary", "parentDoesNotExists"));
            }
            else
            {
                dictionaryItem.ParentId = parent.Key;
                if (dictionaryItem.Key == parent.ParentId)
                    return ValidationProblem(_localizedTextService.Localize("moveOrCopy", "notAllowedByPath"));
            }

            _localizationService.Save(dictionaryItem);

            var model = _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionaryItem);

            return Content(model!.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
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
        public ActionResult<DictionaryDisplay?> PostSave(DictionarySave dictionary)
        {
            var dictionaryItem = dictionary.Id is null ? null :
                _localizationService.GetDictionaryItemById(int.Parse(dictionary.Id.ToString()!, CultureInfo.InvariantCulture));

            if (dictionaryItem == null)
                return ValidationProblem("Dictionary item does not exist");

            var userCulture = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.GetUserCulture(_localizedTextService, _globalSettings);

            if (dictionary.NameIsDirty)
            {
                // if the name (key) has changed, we need to check if the new key does not exist
                var dictionaryByKey = _localizationService.GetDictionaryItemByKey(dictionary.Name!);

                if (dictionaryByKey != null && dictionaryItem.Id != dictionaryByKey.Id)
                {

                    var message = _localizedTextService.Localize(
                        "dictionaryItem","changeKeyError",
                        userCulture,
                        new Dictionary<string, string?> { { "0", dictionary.Name } });
                    ModelState.AddModelError("Name", message);
                    return ValidationProblem(ModelState);
                }

                dictionaryItem.ItemKey = dictionary.Name!;
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

                model?.Notifications.Add(new BackOfficeNotification(
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
                    if (display is not null)
                    {
                        display.Level = level;
                        list.Add(display);
                    }

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
            foreach (var childItem in _localizationService.GetDictionaryItemChildren(dictionaryItem.Key)?.OrderBy(ItemSort()) ?? Enumerable.Empty<IDictionaryItem>())
            {
                var item = _umbracoMapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(childItem);
                if (item is not null)
                {
                    item.Level = level;
                    list.Add(item);
                }

                GetChildItemsForList(childItem, level + 1, list);
            }
        }

        public IActionResult ExportDictionary(int id, bool includeChildren = false)
        {
            var dictionaryItem = _localizationService.GetDictionaryItemById(id);
            if (dictionaryItem == null)
                throw new NullReferenceException("No dictionary item found with id " + id);

            var xml = _serializer.Serialize(dictionaryItem, includeChildren);

            var fileName = $"{dictionaryItem.ItemKey}.udt";
            // Set custom header so umbRequestHelper.downloadFile can save the correct filename
            HttpContext.Response.Headers.Add("x-filename", fileName);

            return File(Encoding.UTF8.GetBytes(xml.ToDataString()), MediaTypeNames.Application.Octet, fileName);
        }

        public IActionResult ImportDictionary(string file, int parentId)
        {
            if (string.IsNullOrWhiteSpace(file))
                return NotFound();

            var filePath = Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data), file);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var xd = new XmlDocument { XmlResolver = null };
            xd.Load(filePath);

            var userId = _backofficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? 0;
            var element = XElement.Parse(xd.InnerXml);

            var parentDictionaryItem = _localizationService.GetDictionaryItemById(parentId);
            var dictionaryItems = _packageDataInstallation.ImportDictionaryItem(element, userId, parentDictionaryItem?.Key);

            // Try to clean up the temporary file.
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up temporary udt file in {File}", filePath);
            }

            var model = _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionaryItems.FirstOrDefault());
            return Content(model!.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        public ActionResult<DictionaryImportModel> Upload(IFormFile file)
        {

            if (file == null)
                return ValidationProblem(
                    _localizedTextService.Localize("media", "failedFileUpload"),
                    _localizedTextService.Localize("speechBubbles", "fileErrorNotFound"));

            var fileName = file.FileName.Trim(Constants.CharArrays.DoubleQuote);
            var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            var root = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
            var tempPath = Path.Combine(root, fileName);

            if (!Path.GetFullPath(tempPath).StartsWith(Path.GetFullPath(root)))
                return ValidationProblem(
                     _localizedTextService.Localize("media", "failedFileUpload"),
                    _localizedTextService.Localize("media", "invalidFileName"));

            if (!ext.InvariantEquals("udt"))
                return ValidationProblem(
                     _localizedTextService.Localize("media", "failedFileUpload"),
                    _localizedTextService.Localize("media", "disallowedFileType"));

            using (var stream = System.IO.File.Create(tempPath))
            {
                file.CopyToAsync(stream).GetAwaiter().GetResult();
            }

            var xd = new XmlDocument
            {
                XmlResolver = null
            };
            xd.Load(tempPath);

            if (xd.DocumentElement == null)
                return ValidationProblem(
                    _localizedTextService.Localize("media", "failedFileUpload"),
                    _localizedTextService.Localize("speechBubbles", "fileErrorNotFound"));

            DictionaryImportModel model = new DictionaryImportModel()
            {
                TempFileName = tempPath,
                DictionaryItems = new List<DictionaryPreviewImportModel>()
            };

            int level = 1;
            string curentParrent = string.Empty;
            foreach (XmlNode dictionaryItem in xd.GetElementsByTagName("DictionaryItem"))
            {
                var name = dictionaryItem.Attributes?.GetNamedItem("Name")?.Value ?? string.Empty;
                var parentKey = dictionaryItem?.ParentNode?.Attributes?.GetNamedItem("Key")?.Value ?? string.Empty;

                if (parentKey != curentParrent || level == 1)
                {
                    level += 1;
                    curentParrent = parentKey;
                }

                model.DictionaryItems.Add(new DictionaryPreviewImportModel()
                {
                    Level = level,
                    Name = name
                });
            }
  
            if (!model.DictionaryItems.Any())
                return ValidationProblem(
                    _localizedTextService.Localize("media", "failedFileUpload"),
                    _localizedTextService.Localize("dictionary", "noItemsInFile"));

            return model;
        }

        private static Func<IDictionaryItem, string> ItemSort() => item => item.ItemKey;
    }
}
