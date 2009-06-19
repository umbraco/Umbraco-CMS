using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Reflection;
using System.IO;
using System.IO.Compression;

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

			byte[] fileBytes = CombineFiles(fileset, context);
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
		private byte[] CombineFiles(string fileList, HttpContext context)
		{
			MemoryStream ms = new MemoryStream(5000);
			//get the file list, and write the contents of each file to the output stream.
			string[] strFiles = DecodeFrom64(fileList).Split(';');
			StreamWriter sw = new StreamWriter(ms);			
			foreach (string s in strFiles)
			{
				if (!string.IsNullOrEmpty(s))
				{
					FileInfo fi = new FileInfo(context.Server.MapPath(s));
					if (!fi.Exists)
						throw new NullReferenceException("File could not be found: " + fi.FullName);
					string fileContents = File.ReadAllText(fi.FullName);
					sw.WriteLine(fileContents);
				}
			}
			sw.Flush();
			byte[] outputBytes = ms.ToArray();
			sw.Close();
			ms.Close();
			return outputBytes;
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
				if (acceptEncoding.Contains("gzip"))
				{
					context.Response.AddHeader("Content-encoding", "gzip");
					compressedStream = new GZipStream(ms, CompressionMode.Compress, true);
					isCompressed = true;
				}
				else if (acceptEncoding.Contains("deflate"))
				{
					context.Response.AddHeader("Content-encoding", "deflate");
					compressedStream = new DeflateStream(ms, CompressionMode.Compress, true);
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

