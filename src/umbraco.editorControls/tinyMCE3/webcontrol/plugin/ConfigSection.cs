/*
 * $Id: ConfigSection.cs 439 2007-11-26 13:26:10Z spocke $
 *
 * @author Moxiecode
 * @copyright Copyright © 2004-2007, Moxiecode Systems AB, All rights reserved.
 */

using System;
using System.Collections.Specialized;

namespace umbraco.editorControls.tinyMCE3.webcontrol.plugin
{
    /// <summary>
	/// Description of ConfigSection.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class ConfigSection
    {
		#region private
		private NameValueCollection globalSettings;
		private bool gzipEnabled, gzipDiskCache, gzipNoCompression;
		private string installPath, mode, gzipCachePath;
		private long gzipExpiresOffset;
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public ConfigSection() {
			this.globalSettings = new NameValueCollection();
		}

		/// <summary>
		/// 
		/// </summary>
		public string InstallPath {
			get { return installPath; }
			set { installPath = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Mode {
			get { return mode; }
			set { mode = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public NameValueCollection GlobalSettings {
			get { return globalSettings; }
			set { globalSettings = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool GzipEnabled {
			get { return gzipEnabled; }
			set { gzipEnabled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public long GzipExpiresOffset {
			get { return gzipExpiresOffset; }
			set { gzipExpiresOffset = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool GzipDiskCache {
			get { return gzipDiskCache; }
			set { gzipDiskCache = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string GzipCachePath {
			get { return gzipCachePath; }
			set { gzipCachePath = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool GzipNoCompression {
			get { return gzipNoCompression; }
			set { gzipNoCompression = value; }
		}
	}
}
