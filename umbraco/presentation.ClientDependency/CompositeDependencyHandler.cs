using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace umbraco.presentation.ClientDependency
{
	public class CompositeDependencyHandler : IHttpHandler
	{
		static CompositeDependencyHandler()
		{
			HandlerFileName = DefaultHandlerFileName;
			MaxHandlerUrlLength = DefaultMaxHandlerUrlLength;
		}

		private static readonly string _versionNo = string.Empty;
		private const string DefaultHandlerFileName = "DependencyHandler.axd";
		private const int DefaultMaxHandlerUrlLength = 2000;

		/// <summary>
		/// The handler file name, by default this is DependencyHandler.axd.
		/// </summary>
		/// <remarks>
		/// If this handler path needs to change, it can be change by setting it in the global.asax on application start
		/// </remarks>
		public static string HandlerFileName { get; set; }

		/// <summary>
		/// When building composite includes, it creates a Base64 encoded string of all of the combined dependency file paths
		/// for a given composite group. If this group contains too many files, then the file path with the query string will be very long.
		/// This is the maximum allowed number of characters that there is allowed, otherwise an exception is thrown.
		/// </summary>
		/// <remarks>
		/// If this handler path needs to change, it can be change by setting it in the global.asax on application start
		/// </remarks>
		public static int MaxHandlerUrlLength { get; set; }

		bool IHttpHandler.IsReusable
		{
			get
			{
				return true;
			}
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			HttpResponse response = context.Response;
			string fileset = context.Server.UrlDecode(context.Request["s"]);
			ClientDependencyType type;
			try
			{
				type = (ClientDependencyType)Enum.Parse(typeof(ClientDependencyType), context.Request["t"], true);
			}
			catch
			{
				throw new ArgumentException("Could not parse the type set in the request");
			}

			if (string.IsNullOrEmpty(fileset))
				throw new ArgumentException("Must specify a fileset in the request");

			byte[] fileBytes = CombineFiles(fileset, context, type);
			byte[] outputBytes = CompressBytes(context, fileBytes);
			SetCaching(context);

			context.Response.ContentType = type == ClientDependencyType.Javascript ? "text/javascript" : "text/css";
			context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);
		}

		/// <summary>
		/// combines all files to a byte array
		/// </summary>
		/// <param name="fileList"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		private byte[] CombineFiles(string fileList, HttpContext context, ClientDependencyType type)
		{
			MemoryStream ms = new MemoryStream(5000);
			//get the file list, and write the contents of each file to the output stream.
			string[] strFiles = DecodeFrom64(fileList).Split(';');
			StreamWriter sw = new StreamWriter(ms);
			foreach (string s in strFiles)
			{
				if (!string.IsNullOrEmpty(s))
				{
					try
					{
						FileInfo fi = new FileInfo(context.Server.MapPath(s));
						if (ClientDependencyHelper.FileBasedDependencyExtensionList.Contains(fi.Extension.ToLower().Replace(".", "")))
						{							
							//if the file doesn't exist, then we'll assume it is a URI external request
							if (!fi.Exists)
							{
								WriteRequestToStream(ref sw, s);
							}
							else
							{
								//if it is a file based dependency then read it
								string fileContents = File.ReadAllText(fi.FullName);
								sw.WriteLine(fileContents);
							}
						}
						else
						{
							//if it's not a file based dependency, try to get the request output.
							WriteRequestToStream(ref sw, s);
						}
					}
					catch (Exception ex)
					{
						Type exType = ex.GetType();
						if (exType.Equals(typeof(NotSupportedException)) || exType.Equals(typeof(ArgumentException)))
						{
							//could not parse the string into a fileinfo, so we assume it is a URI
							WriteRequestToStream(ref sw, s);
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
			return outputBytes;
		}

		/// <summary>
		/// Writes the output of an external request to the stream. Returns true/false if succesful or not.
		/// </summary>
		/// <param name="sw"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		private bool WriteRequestToStream(ref StreamWriter sw, string url)
		{
			string requestOutput;
			bool rVal = false;
			rVal = TryReadUri(url, out requestOutput);
			if (rVal)
			{
				//write the contents of the external request.
				sw.WriteLine(requestOutput);
			}
			return rVal;
		}

		/// <summary>
		/// Tries to convert the url to a uri, then read the request into a string and return it.
		/// This takes into account relative vs absolute URI's
		/// </summary>
		/// <param name="url"></param>
		/// <param name="requestContents"></param>
		/// <returns></returns>
		private bool TryReadUri(string url, out string requestContents)
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
		/// Sets the output cache parameters and also the client side caching parameters
		/// </summary>
		/// <param name="context"></param>
		private void SetCaching(HttpContext context)
		{
			//This ensures OutputCaching is set for this handler and also controls
			//client side caching on the browser side. Default is 10 days.
			TimeSpan duration = TimeSpan.FromDays(10);
			HttpCachePolicy cache = context.Response.Cache;
			cache.SetCacheability(HttpCacheability.Public);
			cache.SetExpires(DateTime.Now.Add(duration));
			cache.SetMaxAge(duration);
			cache.SetValidUntilExpires(true);
			cache.SetLastModified(DateTime.Now);
			cache.SetETag(Guid.NewGuid().ToString());
			//set server OutputCache to vary by our params
			cache.VaryByParams["t"] = true;
			cache.VaryByParams["s"] = true;
			//don't allow varying by wildcard
			cache.SetOmitVaryStar(true);
			//ensure client browser maintains strict caching rules
			cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
			//This is the only way to set the max-age cachability header in ASP.Net!
			FieldInfo maxAgeField = cache.GetType().GetField("_maxAge", BindingFlags.Instance | BindingFlags.NonPublic);
			maxAgeField.SetValue(cache, duration);
		}

		/// <summary>
		/// Compresses the bytes if the browser supports it
		/// </summary>
		private byte[] CompressBytes(HttpContext context, byte[] fileBytes)
		{
			string acceptEncoding = context.Request.Headers["Accept-Encoding"];
			if (!string.IsNullOrEmpty(acceptEncoding))
			{
				MemoryStream ms = new MemoryStream();
				Stream compressedStream = null;
				acceptEncoding = acceptEncoding.ToLowerInvariant();
				bool isCompressed = false;
				
				//deflate is faster in .Net according to Mads Kristensen (blogengine.net)
				if (acceptEncoding.Contains("deflate"))
				{
					context.Response.AddHeader("Content-encoding", "deflate");
					compressedStream = new DeflateStream(ms, CompressionMode.Compress, true);
					isCompressed = true;
				}
				else if (acceptEncoding.Contains("gzip"))
				{
					context.Response.AddHeader("Content-encoding", "gzip");
					compressedStream = new GZipStream(ms, CompressionMode.Compress, true);
					isCompressed = true;
				}
				
				if (isCompressed)
				{
					//write the bytes to the compressed stream
					compressedStream.Write(fileBytes, 0, fileBytes.Length);
					compressedStream.Close();
					byte[] outputBytes = ms.ToArray();
					ms.Close();
					return outputBytes;
				}
			}
			//not compressed
			return fileBytes;
		}

		private string DecodeFrom64(string toDecode)
		{
			byte[] toDecodeAsBytes = System.Convert.FromBase64String(toDecode);
			return System.Text.ASCIIEncoding.ASCII.GetString(toDecodeAsBytes);
		}
	}
}

