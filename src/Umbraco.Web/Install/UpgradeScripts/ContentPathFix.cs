using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Install.UpgradeScripts
{
    /// <summary>
    /// An upgrade script to fix a moving issue in 4.10+
    /// http://issues.umbraco.org/issue/U4-1491
    /// </summary>
    public class ContentPathFix : IUpgradeScript
    {
        private readonly StringBuilder _report = new StringBuilder();

        public void Execute()
        {
            //return;
            if (ApplicationContext.Current == null) return;
            if (HasBeenFixed()) return;
            Fix();
            WriteReport();
        }

        private void Fix()
        {
            AddReportLine("Starting fix paths script");

            //fix content
            AddReportLine("Fixing content");
            foreach (var d in Document.GetRootDocuments())
            {
                FixPathsForChildren(d, content => ((Document)content).Children);
            }
            AddReportLine("Fixing content recycle bin");
            var contentRecycleBin = new RecycleBin(RecycleBin.RecycleBinType.Content);
            foreach (var d in contentRecycleBin.Children)
            {
                FixPathsForChildren(new Document(d.Id), content => ((Document)content).Children);
            }

            //fix media
            AddReportLine("Fixing media");
            foreach (var d in global::umbraco.cms.businesslogic.media.Media.GetRootMedias())
            {
                FixPathsForChildren(d, media => ((global::umbraco.cms.businesslogic.media.Media)media).Children);
            }
            AddReportLine("Fixing media recycle bin");
            var mediaRecycleBin = new RecycleBin(RecycleBin.RecycleBinType.Media);
            foreach (var d in mediaRecycleBin.Children)
            {
                FixPathsForChildren(new global::umbraco.cms.businesslogic.media.Media(d.Id), media => ((global::umbraco.cms.businesslogic.media.Media)media).Children);
            }
            AddReportLine("Complete!");
        }

        /// <summary>
        /// Returns true if this script has run based on a temp file written to
        /// ~/App_Data/TEMP/FixPaths/report.txt
        /// </summary>
        /// <returns></returns>
        private bool HasBeenFixed()
        {
            return File.Exists(IOHelper.MapPath("~/App_Data/TEMP/FixPaths/report.txt"));
        }

        /// <summary>
        /// Creates the report
        /// </summary>
        private void WriteReport()
        {
            var filePath = IOHelper.MapPath("~/App_Data/TEMP/FixPaths/report.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var writer = File.CreateText(IOHelper.MapPath("~/App_Data/TEMP/FixPaths/report.txt")))
            {
                writer.Write(_report.ToString());
            }
        }

        /// <summary>
        /// Recursively iterates over the children of the document and fixes the path
        /// </summary>
        /// <param name="d"></param>
        /// <param name="getChildren">Callback to get the children of the conent item</param>
        /// <remarks>
        /// We cannot use GetDescendants() because that is based on the paths of documents and if they are invalid then
        /// we cannot use that method.
        /// </remarks>
        private void FixPathsForChildren(Content d, Func<Content, IEnumerable<Content>> getChildren)
        {
            AddReportLine("Fixing paths for children of " + d.Id);
            foreach (var c in getChildren(d))
            {
                FixPath(c);
                if (c.HasChildren)
                {
                    FixPathsForChildren(c, getChildren);
                }
            }
        }

        /// <summary>
        /// Check if the path is correct based on the document's parent if it is not correct, then fix it
        /// </summary>
        /// <param name="d"></param>
        private void FixPath(CMSNode d)
        {
            AddReportLine("Checking path for " + d.Id + ". Current = " + d.Path);
            //check if the path is correct
            var correctpath = d.Parent.Path + "," + d.Id.ToString();
            if (d.Path != correctpath)
            {
                AddReportLine(" INVALID PATH DETECTED. Path for " + d.Id + " changed to: " + d.Parent.Path + "," + d.Id.ToString());
                d.Path = correctpath;
                d.Level = d.Parent.Level + 1;
            }
        }

        private void AddReportLine(string str)
        {
            _report.AppendLine(string.Format("{0} - " + str, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
        }

        
    }
}