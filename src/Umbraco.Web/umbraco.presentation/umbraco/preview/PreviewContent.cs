﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace umbraco.presentation.preview
{
    public enum PreviewMode
    {
        Unknown = 0, // default value
        Legacy,
        Default
    }

    public class PreviewContent
    {
        private static PreviewMode _previewMode;
        private const PreviewMode DefaultPreviewMode = PreviewMode.Default;
        private static int _singlePreviewCacheDurationSeconds = -1;
        private const int DefaultSinglePreviewCacheDurationSeconds = 60;

        public static PreviewMode PreviewMode
        {
            get
            {
                if (_previewMode != PreviewMode.Unknown)
                    return _previewMode;

                var appSettings = ConfigurationManager.AppSettings;
                var setting = appSettings["Umbraco.Preview.Mode"];
                if (setting.IsNullOrWhiteSpace())
                    return _previewMode = DefaultPreviewMode;
                if (Enum<PreviewMode>.TryParse(setting, false, out _previewMode))
                    return _previewMode;
                throw new ConfigurationErrorsException($"Failed to parse Umbraco.Preview.Mode appSetting, {setting} is not a valid value. "
                    + "Valid values are: Vintage (default), SinglePreview.");
            }
        }

        public static int SinglePreviewCacheDurationSeconds
        {
            get
            {
                if (_singlePreviewCacheDurationSeconds >= 0)
                    return _singlePreviewCacheDurationSeconds;

                var appSettings = ConfigurationManager.AppSettings;
                var setting = appSettings["Umbraco.Preview.SinglePreview.CacheDurationSeconds"];
                if (setting.IsNullOrWhiteSpace())
                    return _singlePreviewCacheDurationSeconds = DefaultSinglePreviewCacheDurationSeconds;
                if (int.TryParse(setting, out _singlePreviewCacheDurationSeconds))
                    return _singlePreviewCacheDurationSeconds;
                throw new ConfigurationErrorsException($"Failed to parse Umbraco.Preview.SinglePreview.CacheDurationSeconds appSetting, {setting} is not a valid value. "
                    + "Valid values are positive integers.");
            }
        }

        public static bool IsSinglePreview => PreviewMode == PreviewMode.Default;

        public XmlDocument XmlContent { get; set; }
        public Guid PreviewSet { get; set; }
        public string PreviewsetPath { get; set; }

        public bool ValidPreviewSet { get; set; }

        private int _userId = -1;

        public PreviewContent()
        {
            _initialized = false;
        }

        private readonly object _initLock = new object();
        private bool _initialized = true;

        public void EnsureInitialized(User user, string previewSet, bool validate, Action initialize)
        {
            if (IsSinglePreview) return;

            lock (_initLock)
            {
                if (_initialized) return;

                _userId = user.Id;
                ValidPreviewSet = UpdatePreviewPaths(new Guid(previewSet), validate);
                initialize();
                _initialized = true;
            }
        }

        public PreviewContent(User user)
        {
            _userId = user.Id;
        }

        public PreviewContent(Guid previewSet)
        {
            ValidPreviewSet = IsSinglePreview || UpdatePreviewPaths(previewSet, true);
        }
        public PreviewContent(User user, Guid previewSet, bool validate)
        {
            _userId = user.Id;
            ValidPreviewSet = IsSinglePreview || UpdatePreviewPaths(previewSet, validate);
        }


        public void PrepareDocument(User user, Document documentObject, bool includeSubs)
        {
            if (IsSinglePreview) return;

            _userId = user.Id;

            // clone xml
            XmlContent = (XmlDocument)content.Instance.XmlContent.Clone();

            var previewNodes = new List<Document>();

            var parentId = documentObject.Level == 1 ? -1 : documentObject.ParentId;

            while (parentId > 0 && XmlContent.GetElementById(parentId.ToString(CultureInfo.InvariantCulture)) == null)
            {
                var document = new Document(parentId);
                previewNodes.Insert(0, document);
                parentId = document.ParentId;
            }

            previewNodes.Add(documentObject);

            foreach (var document in previewNodes)
            {
                //Inject preview xml
                parentId = document.Level == 1 ? -1 : document.ParentId;
                var previewXml = document.ToPreviewXml(XmlContent);
                if (document.ContentEntity.Published == false
                    && ApplicationContext.Current.Services.ContentService.HasPublishedVersion(document.Id))
                    previewXml.Attributes.Append(XmlContent.CreateAttribute("isDraft"));
                XmlContent = content.GetAddOrUpdateXmlNode(XmlContent, document.Id, document.Level, parentId, previewXml);
            }

            if (includeSubs)
            {
                foreach (var prevNode in documentObject.GetNodesForPreview(true))
                {
                    var previewXml = XmlContent.ReadNode(XmlReader.Create(new StringReader(prevNode.Xml)));
                    if (prevNode.IsDraft)
                        previewXml.Attributes.Append(XmlContent.CreateAttribute("isDraft"));
                    XmlContent = content.GetAddOrUpdateXmlNode(XmlContent, prevNode.NodeId, prevNode.Level, prevNode.ParentId, previewXml);
                }
            }

        }

        private bool UpdatePreviewPaths(Guid previewSet, bool validate)
        {
            if (_userId == -1)
            {
                throw new ArgumentException("No current Umbraco User registered in Preview", "m_userId");
            }

            PreviewSet = previewSet;
            PreviewsetPath = GetPreviewsetPath(_userId, previewSet);

            if (validate && !ValidatePreviewPath())
            {
                // preview cookie failed so we'll log the error and clear the cookie
                LogHelper.Debug<PreviewContent>(string.Format("Preview failed for preview set {0} for user {1}", previewSet, _userId));

                PreviewSet = Guid.Empty;
                PreviewsetPath = String.Empty;

                ClearPreviewCookie();

                return false;
            }

            return true;
        }

        private static string GetPreviewsetPath(int userId, Guid previewSet)
        {
            return IOHelper.MapPath(
                Path.Combine(SystemDirectories.Preview, userId + "_" + previewSet + ".config"));
        }

        /// <summary>
        /// Checks a preview file exist based on preview cookie
        /// </summary>
        /// <returns></returns>
        public bool ValidatePreviewPath()
        {
            if (IsSinglePreview) return true;

            if (!File.Exists(PreviewsetPath))
                return false;

            return true;
        }



        public void LoadPreviewset()
        {
            if (IsSinglePreview)
            {
                XmlContent = content.Instance.PreviewXmlContent;
            }
            else
            {
                XmlContent = new XmlDocument();
                XmlContent.Load(PreviewsetPath);
            }
        }

        public void SavePreviewSet()
        {
            if (IsSinglePreview) return;

            //make sure the preview folder exists first
            var dir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Preview));
            if (!dir.Exists)
            {
                dir.Create();
            }

            // check for old preview sets and try to clean
            CleanPreviewDirectory(_userId, dir);

            XmlContent.Save(PreviewsetPath);
        }

        private static void CleanPreviewDirectory(int userId, DirectoryInfo dir)
        {
            // also delete any files accessed more than 10 minutes ago
            var now = DateTime.Now;
            foreach (var file in dir.GetFiles("*.config"))
            {
                if ((now - file.LastAccessTime).TotalMinutes > 10)
                    DeletePreviewFile(userId, file);
            }
        }

        private static void DeletePreviewFile(int userId, FileInfo file)
        {
            try
            {
                file.Delete();
            }
            catch (IOException)
            {
                // for *some* reason deleting the file can fail,
                // and it will work later on (long-lasting locks, etc),
                // so just ignore the exception
            }
            catch (Exception ex)
            {
                LogHelper.Error<PreviewContent>(string.Format("Couldn't delete preview set: {0} - User {1}", file.Name, userId), ex);
            }
        }

        public void ActivatePreviewCookie()
        {
            // zb-00004 #29956 : refactor cookies names & handling
            StateHelper.Cookies.Preview.SetValue(PreviewSet.ToString());
        }

        public static void ClearPreviewCookie()
        {
            // zb-00004 #29956 : refactor cookies names & handling
            if (!IsSinglePreview && UmbracoContext.Current.UmbracoUser != null)
            {
                if (StateHelper.Cookies.Preview.HasValue)
                {

                    DeletePreviewFile(
                        UmbracoContext.Current.UmbracoUser.Id,
                        new FileInfo(GetPreviewsetPath(
                            UmbracoContext.Current.UmbracoUser.Id,
                            new Guid(StateHelper.Cookies.Preview.GetValue()))));
                }
            }
            StateHelper.Cookies.Preview.Clear();
        }
    }
}
