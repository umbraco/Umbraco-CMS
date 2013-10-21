/*
 * $Id: GzipCompressor.cs 439 2007-11-26 13:26:10Z spocke $
 *
 * @author Moxiecode
 * @copyright Copyright © 2004-2007, Moxiecode Systems AB, All rights reserved.
 */

using System;
using System.Web;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace umbraco.editorControls.tinyMCE3.webcontrol.plugin
{
    /// <summary>
	/// Description of GzipCompressor.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class GzipCompressor
    {
		#region private
		private bool diskCache, noCompression;
		private string cachePath;
		private ArrayList items;
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public GzipCompressor() {
			this.items = new ArrayList();
		}

		/// <summary>
		/// 
		/// </summary>
		public bool NoCompression {
			get { return noCompression; }
			set { noCompression = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool DiskCache {
			get { return diskCache; }
			set { diskCache = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string CachePath {
			get { return cachePath; }
			set { cachePath = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void AddFile(string path) {
			//System.Web.HttpContext.Current.Response.Write(path + "\n");
			if (File.Exists(path))
				this.items.Add(new CompressItem(CompressItemType.File, path));
			else
				throw new Exception("File could not be found \"" + path + "\", unable to add it for Gzip compression.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void AddData(string data) {
			this.items.Add(new CompressItem(CompressItemType.Chunk, data));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="to_stream"></param>
		public void Compress(Stream to_stream) {
			GZipStream gzipStream = null;
			string key = "";
			byte[] bytes;

			// Build cache key
			foreach (CompressItem item in this.items) {
				if (item.Type == CompressItemType.File)
					key += item.Data;
			}

			key = MD5(key);
			if (this.NoCompression) {
				this.SendPlainText(key, to_stream);
				return;
			}

			// Check for cached file on disk, stream that one if it was found
			if (this.DiskCache && File.Exists(Path.Combine(this.CachePath, key + ".gz"))) {
				this.StreamFromTo(File.OpenRead(Path.Combine(this.CachePath, key + ".gz")), to_stream, 4096, CloseMode.InStream);
				return;
			}

			try {
				// Build new gzipped stream
				if (this.DiskCache) {
						gzipStream = new GZipStream(File.OpenWrite(Path.Combine(this.CachePath, key + ".gz")), CompressionMode.Compress);

						// Compress all files into memory
						foreach (CompressItem item in this.items) {
							// Add file
							if (item.Type == CompressItemType.File)
								StreamFromTo(File.OpenRead(item.Data), gzipStream, 4096, CloseMode.InStream);

							// Add chunk
							if (item.Type == CompressItemType.Chunk) {
								bytes = Encoding.ASCII.GetBytes(item.Data.ToCharArray());
								gzipStream.Write(bytes, 0, bytes.Length);
							}
						}

						// Close gzip stream
						gzipStream.Close();
						gzipStream = null;

						// Send cached file to user
						this.StreamFromTo(File.OpenRead(Path.Combine(this.CachePath, key + ".gz")), to_stream, 4096, CloseMode.InStream);
				} else {
					gzipStream = new GZipStream(to_stream, CompressionMode.Compress);

					// Compress all files into output stream
					foreach (CompressItem item in this.items) {
						// Add file
						if (item.Type == CompressItemType.File)
							StreamFromTo(File.OpenRead(item.Data), gzipStream, 4096, CloseMode.InStream);

						// Add chunk
						if (item.Type == CompressItemType.Chunk) {
							bytes = Encoding.ASCII.GetBytes(item.Data.ToCharArray());
							gzipStream.Write(bytes, 0, bytes.Length);
						}
					}
				}
			} finally {
				if (gzipStream != null)
					gzipStream.Close();
			}
		}

		#region private

		private void SendPlainText(string key, Stream to_stream) {
			Stream fileStream = null;
			byte[] bytes;

			// Check for cached file on disk, stream that one if it was found
			if (this.DiskCache && File.Exists(Path.Combine(this.CachePath, key + ".js"))) {
				this.StreamFromTo(File.OpenRead(Path.Combine(this.CachePath, key + ".js")), to_stream, 4096, CloseMode.InStream);
				return;
			}

			// Build new plain text stream
			if (this.DiskCache) {
				try {
					fileStream = File.OpenWrite(Path.Combine(this.CachePath, key + ".js"));

					// Compress all files into memory
					foreach (CompressItem item in this.items) {
						// Add file
						if (item.Type == CompressItemType.File)
							StreamFromTo(File.OpenRead(item.Data), fileStream, 4096, CloseMode.InStream);

						// Add chunk
						if (item.Type == CompressItemType.Chunk) {
							bytes = Encoding.ASCII.GetBytes(item.Data.ToCharArray());
							fileStream.Write(bytes, 0, bytes.Length);
						}
					}

					// Close file stream
					fileStream.Close();
					fileStream = null;
				} finally {
					// Close file stream
					if (fileStream != null)
						fileStream.Close();
				}

				// Send cached file to user
				this.StreamFromTo(File.OpenRead(Path.Combine(this.CachePath, key + ".js")), to_stream, 4096, CloseMode.InStream);
			} else {
				// Concat all files into output stream
				foreach (CompressItem item in this.items) {
					// Add file
					if (item.Type == CompressItemType.File)
						StreamFromTo(File.OpenRead(item.Data), to_stream, 4096, CloseMode.InStream);

					// Add chunk
					if (item.Type == CompressItemType.Chunk) {
						bytes = Encoding.ASCII.GetBytes(item.Data.ToCharArray());
						to_stream.Write(bytes, 0, bytes.Length);
					}
				}
			}
		}

		private void StreamFromTo(Stream in_stream, Stream out_stream, int buff_size, CloseMode mode) {
			byte[] buff = new byte[buff_size];
			int len;

			try {
				while ((len = in_stream.Read(buff, 0, buff_size)) > 0) {
					out_stream.Write(buff, 0, len);
					out_stream.Flush();
				}
			} finally {
				if (in_stream != null && (mode == CloseMode.Both || mode == CloseMode.InStream))
					in_stream.Close();

				if (out_stream != null && (mode == CloseMode.Both || mode == CloseMode.OutStream))
					out_stream.Close();
			}
		}

		private string MD5(string str) {
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(str));
			str = BitConverter.ToString(result);

			return str.Replace("-", "");
		}

		#endregion
	}

	enum CloseMode {
		None, InStream, OutStream, Both
	}

	enum CompressItemType {
		File, Chunk
	}

	class CompressItem {
		private string data;
		private CompressItemType type;

		public CompressItem(CompressItemType type, string data) {
			this.type = type;
			this.data = data;
		}

		public string Data {
			get { return data; }
			set { data = value; }
		}

		public CompressItemType Type {
			get { return type; }
		}
	}
}
