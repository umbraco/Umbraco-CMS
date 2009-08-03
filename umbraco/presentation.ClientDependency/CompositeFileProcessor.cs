using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.presentation.ClientDependency.Config;
using System.IO;
using System.Web;
using System.Net;
using System.IO.Compression;

namespace umbraco.presentation.ClientDependency
{
	public enum CompressionType
	{
		deflate, gzip, none
	}

	/// <summary>
	/// A simple class defining a Uri string and whether or not it is a local application file
	/// </summary>
	public class CompositeFileDefinition
	{
		public CompositeFileDefinition(string uri, bool isLocalFile)
		{
			IsLocalFile = isLocalFile;
			Uri = uri;
		}
		public bool IsLocalFile { get; set; }
		public string Uri { get; set; }

		public override bool Equals(object obj)
		{
			return (obj.GetType().Equals(this.GetType())
				&& ((CompositeFileDefinition)obj).IsLocalFile.Equals(IsLocalFile)
				&& ((CompositeFileDefinition)obj).Uri.Equals(Uri));
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode();
		}
	}

	/// <summary>
	/// A utility class for combining, compressing and saving composite scripts/css files
	/// </summary>
	public static class CompositeFileProcessor
	{

		/// <summary>
		/// Saves the file's bytes to disk with a hash of the byte array
		/// </summary>
		/// <param name="fileContents"></param>
		/// <param name="type"></param>
		/// <returns>The new file path</returns>
		/// <remarks>
		/// the extension will be: .cdj for JavaScript and .cdc for CSS
		/// </remarks>
		public static string SaveCompositeFile(byte[] fileContents, ClientDependencyType type)
		{
            //don't save the file if composite files are disabled.
            if (!ClientDependencySettings.Instance.EnableCompositeFiles)
                return string.Empty;

			if (!ClientDependencySettings.Instance.CompositeFilePath.Exists)
				ClientDependencySettings.Instance.CompositeFilePath.Create();
			FileInfo fi = new FileInfo(
				Path.Combine(ClientDependencySettings.Instance.CompositeFilePath.FullName,
					fileContents.GetHashCode().ToString() + ".cd" + type.ToString().Substring(0, 1).ToLower()));
			if (fi.Exists)
				fi.Delete();
			FileStream fs = fi.Create();
			fs.Write(fileContents, 0, fileContents.Length);
			fs.Close();
			return fi.FullName;
		}


		/// <summary>
		/// combines all files to a byte array
		/// </summary>
		/// <param name="fileList"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static byte[] CombineFiles(string[] strFiles, HttpContext context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs)
		{

			List<CompositeFileDefinition> fDefs = new List<CompositeFileDefinition>();

			MemoryStream ms = new MemoryStream(5000);
			StreamWriter sw = new StreamWriter(ms);
			foreach (string s in strFiles)
			{
				if (!string.IsNullOrEmpty(s))
				{
					try
					{
						FileInfo fi = new FileInfo(context.Server.MapPath(s));
						if (ClientDependencySettings.Instance.FileBasedDependencyExtensionList.Contains(fi.Extension.ToLower().Replace(".", "")))
						{
							//if the file doesn't exist, then we'll assume it is a URI external request
							if (!fi.Exists)
							{
								WriteFileToStream(ref sw, s, type, ref fDefs);
							}
							else
							{
								WriteFileToStream(ref sw, fi, type, s, ref fDefs);
							}
						}
						else
						{
							//if it's not a file based dependency, try to get the request output.
							WriteFileToStream(ref sw, s, type, ref fDefs);
						}
					}
					catch (Exception ex)
					{
						Type exType = ex.GetType();
						if (exType.Equals(typeof(NotSupportedException)) 
							|| exType.Equals(typeof(ArgumentException))
							|| exType.Equals(typeof(HttpException)))
						{
							//could not parse the string into a fileinfo or couldn't mappath, so we assume it is a URI
							WriteFileToStream(ref sw, s, type, ref fDefs);
						}
						else
						{
							//if this fails, log the exception in trace, but continue
							HttpContext.Current.Trace.Warn("ClientDependency", "Could not load file contents from " + s, ex);
							System.Diagnostics.Debug.Assert(false, "Could not load file contents from " + s, ex.Message);
						}
					}
				}

				if (type == ClientDependencyType.Javascript)
				{
					sw.Write(";;;"); //write semicolons in case the js isn't formatted correctly. This also helps for debugging.
				}

			}
			sw.Flush();
			byte[] outputBytes = ms.ToArray();
			sw.Close();
			ms.Close();
			fileDefs = fDefs;
			return outputBytes;
		}

		/// <summary>
		/// Writes the output of an external request to the stream. Returns true/false if succesful or not.
		/// </summary>
		/// <param name="sw"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		private static bool WriteFileToStream(ref StreamWriter sw, string url, ClientDependencyType type, ref List<CompositeFileDefinition> fileDefs)
		{
			string requestOutput;
			bool rVal = false;
			rVal = TryReadUri(url, out requestOutput);
			if (rVal)
			{
				//write the contents of the external request.
				sw.WriteLine(ParseFileContents(requestOutput, type, url));
				fileDefs.Add(new CompositeFileDefinition(url, false));
			}
			return rVal;
		}

		private static bool WriteFileToStream(ref StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, ref List<CompositeFileDefinition> fileDefs)
		{
			try
			{
				//if it is a file based dependency then read it
				string fileContents = File.ReadAllText(fi.FullName);
				sw.WriteLine(ParseFileContents(fileContents, type, origUrl));
				fileDefs.Add(new CompositeFileDefinition(origUrl, true));
				return true;
			}
			catch (Exception ex)
			{
				HttpContext.Current.Trace.Warn("ClientDependency", "Could not write file " + fi.FullName + " contents to stream", ex);
				System.Diagnostics.Debug.Assert(false, "Could not write file " + fi.FullName + " contents to stream", ex.Message);
				return false;
			}			
		}

		/// <summary>
		/// Currently this only parses CSS files, but potentially could have other uses.
		/// This is the final process before writing to the stream.
		/// </summary>
		/// <param name="fileContents"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private static string ParseFileContents(string fileContents, ClientDependencyType type, string url)
		{
			//if it is a CSS file we need to parse the URLs
			if (type == ClientDependencyType.Css)
			{

				fileContents = CssFileUrlFormatter.TransformCssFile(fileContents, MakeUri(url));
			}
			return fileContents;
		}

		/// <summary>
		/// Checks if the url is a local/relative uri, if it is, it makes it absolute based on the 
		/// current request uri.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private static Uri MakeUri(string url)
		{
			Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
			if (!uri.IsAbsoluteUri)
			{
				string http = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
				Uri absoluteUrl = new Uri(new Uri(http), uri);
				return absoluteUrl;
			}
			return uri;
		}

		/// <summary>
		/// Tries to convert the url to a uri, then read the request into a string and return it.
		/// This takes into account relative vs absolute URI's
		/// </summary>
		/// <param name="url"></param>
		/// <param name="requestContents"></param>
		/// <returns></returns>
		private static bool TryReadUri(string url, out string requestContents)
		{
			Uri uri;
			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
			{
				if (uri.IsAbsoluteUri)
				{
					WebClient client = new WebClient();
					try
					{
						requestContents = client.DownloadString(uri.AbsoluteUri);
						return true;
					}
					catch (Exception ex)
					{
						HttpContext.Current.Trace.Warn("ClientDependency", "Could not load file contents from " + url, ex);
						System.Diagnostics.Debug.Assert(false, "Could not load file contents from " + url, ex.Message);
					}
				}
				else
				{
					//its a relative path so use the execute method
					StringWriter sw = new StringWriter();
					try
					{
						HttpContext.Current.Server.Execute(url, sw);
						requestContents = sw.ToString();
						sw.Close();
						return true;
					}
					catch (Exception ex)
					{
						HttpContext.Current.Trace.Warn("ClientDependency", "Could not load file contents from " + url, ex);
						System.Diagnostics.Debug.Assert(false, "Could not load file contents from " + url, ex.Message);
					}
				}

			}
			requestContents = "";
			return false;
		}

		/// <summary>
		/// Compresses the bytes if the browser supports it
		/// </summary>
		public static byte[] CompressBytes(CompressionType type, byte[] fileBytes)
		{
			MemoryStream ms = new MemoryStream();
			Stream compressedStream = null;

			//deflate is faster in .Net according to Mads Kristensen (blogengine.net)
			if (type == CompressionType.deflate)
			{
				compressedStream = new DeflateStream(ms, CompressionMode.Compress, true);
			}
			else if (type == CompressionType.gzip)
			{
				compressedStream = new GZipStream(ms, CompressionMode.Compress, true);
			}

			if (type != CompressionType.none)
			{
				//write the bytes to the compressed stream
				compressedStream.Write(fileBytes, 0, fileBytes.Length);
				compressedStream.Close();
				byte[] output = ms.ToArray();
				ms.Close();
				return output;
			}

			//not compressed
			return fileBytes;
		}
	}
}
