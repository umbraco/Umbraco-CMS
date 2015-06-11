using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using umbraco.BusinessLogic.Utils;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.cache;
using System.Web.Caching;
using Umbraco.Core.IO;
using umbraco.interfaces;


namespace umbraco.presentation
{
	/// <summary>
	/// Summary description for requestModule.
	/// </summary>
	[Obsolete("This class is not used anymore and will be removed from the codebase in future versions.")]
	public class requestModule : IHttpModule
	{
		protected static Timer publishingTimer;
		protected static Timer pingTimer;

		private HttpApplication mApp;
		private IContainer components = null;

	    private readonly IList<IApplicationStartupHandler> startupHandlers = new List<IApplicationStartupHandler>(); 

		/// <summary>True if the module is currently handling an error.</summary>
		private static object handlingError = false;
		/// <summary>List of errors that occurred since the last error was being handled.</summary>
		private List<Exception> unhandledErrors = new List<Exception>();

		public const string ORIGINAL_URL_CXT_KEY = "umbOriginalUrl";

		private static string LOG_SCRUBBER_TASK_NAME = "ScrubLogs";
		private static CacheItemRemovedCallback OnCacheRemove = null;

		protected void ApplicationStart(HttpApplication HttpApp)
		{
			//starting the application. Application doesn't support HttpContext in integrated mode (standard mode on IIS7) 
			//So everything is moved to beginRequest.
		}

		protected void Application_PostResolveRequestCache(object sender, EventArgs e)
		{
			// process rewrite here so forms authentication can Authorize based on url before the original url is discarded
			this.UmbracoRewrite(sender, e);
		}

		protected void Application_AuthorizeRequest(object sender, EventArgs e)
		{
			// nothing needs to be done here
		}

		protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
		{

			HttpContext context = mApp.Context;
			//httpContext.RewritePath( (string) httpContext.Items[ORIGINAL_URL_CXT_KEY] + "?" + httpContext.Request.QueryString );
		}

		/// <summary>
		/// Handles the BeginRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Application_BeginRequest(Object sender, EventArgs e)
		{
		   
			HttpApplication app = (HttpApplication)sender;

			//first time init, starts timers, and sets httpContext
			InitializeApplication(app);

			// grab the original url before anything modifies  it
			HttpContext httpContext = mApp.Context;
			httpContext.Items[ORIGINAL_URL_CXT_KEY] = rewrite404Url(httpContext.Request.Url.AbsolutePath, httpContext.Request.Url.Query, false);

			// create the Umbraco context
			UmbracoContext.Current = new UmbracoContext(httpContext);

			// rewrite will happen after authorization
		}

		protected string rewrite404Url(string url, string querystring, bool returnQuery)
		{
			// adding endswith and contains checks to ensure support for custom 404 messages (only 404 parse directory and aspx requests)
			if (querystring.StartsWith("?404") && (!querystring.Contains(".") || querystring.EndsWith(".aspx") || querystring.Contains(".aspx&")))
			{
				Uri u = new Uri(querystring.Substring(5, querystring.Length - 5));
				string path = u.AbsolutePath;
				if (returnQuery)
				{
					return u.Query;
				}
				else
				{
					return path;
				}
			}

			if (returnQuery)
			{
				return querystring;
			}
			else
			{
				return url;
			}
		}

		/// <summary>
		/// Performs path rewriting.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void UmbracoRewrite(Object sender, EventArgs e)
		{
			HttpContext context = mApp.Context;
			string path = rewrite404Url(context.Request.Path.ToLower(), context.Request.Url.Query, false);
			string query = String.Empty;

			if (GlobalSettings.UseDirectoryUrls)
			{
				// zb-00017 #29930 : do not change url casing when rewriting
				string requestPath = context.Request.Path; // not lowercased
				int asmxPos = path.IndexOf(".asmx/");
				if (asmxPos >= 0)
					context.RewritePath(path.Substring(0, asmxPos + 5),
										requestPath.Substring(asmxPos + 5),
										context.Request.QueryString.ToString());
			}

			if (path.IndexOf(".aspx") > -1 || path.IndexOf('.') == -1)
			{
				// validate configuration
				if (mApp.Application["umbracoNeedConfiguration"] == null)
					mApp.Application["umbracoNeedConfiguration"] = !GlobalSettings.Configured;

				if (!GlobalSettings.IsReservedPathOrUrl(path))
				{
					// redirect if Umbraco needs configuration
					Nullable<bool> needsConfiguration = (Nullable<bool>)mApp.Application["umbracoNeedConfiguration"];

					if (needsConfiguration.HasValue && needsConfiguration.Value)
					{
						string url = SystemDirectories.Install;
						string meh = IOHelper.ResolveUrl(url);
						string installUrl = string.Format("{0}/default.aspx?redir=true&url={1}", IOHelper.ResolveUrl( SystemDirectories.Install ), context.Request.Path.ToLower());
						context.Response.Redirect(installUrl, true);
					}

					// show splash?
                    else if (UmbracoConfig.For.UmbracoSettings().Content.EnableSplashWhileLoading && content.Instance.isInitializing)
						context.RewritePath(string.Format("{0}/splashes/booting.aspx", SystemDirectories.Config));
					// rewrite page path
					else
					{
						string receivedQuery = rewrite404Url(context.Request.Path.ToLower(), context.Request.Url.Query, true);
						if (receivedQuery.Length > 0)
						{
							// Clean umbPage from querystring, caused by .NET 2.0 default Auth Controls
							if (receivedQuery.IndexOf("umbPage") > 0)
							{
								int ampPos = receivedQuery.IndexOf('&');
								// query contains no ampersand?
								if (ampPos < 0)
								{
									// no ampersand means no original query string
									query = String.Empty;
									// ampersand would occur past then end the of received query
									ampPos = receivedQuery.Length;
								}
								else
								{
									// original query string past ampersand
									query = receivedQuery.Substring(ampPos + 1,
																	receivedQuery.Length - ampPos - 1);
								}
								// get umbPage out of query string (9 = "&umbPage".Length() + 1)
								path = receivedQuery.Substring(9, ampPos - 9); //this will fail if there are < 9 characters before the &umbPage query string
							}
							else
							{
								// strip off question mark
								query = receivedQuery.Substring(1);
							}
						}

						// Add questionmark to query string if it's not empty
						if (!String.IsNullOrEmpty(query))
							query = "?" + query;

						// save original URL
						context.Items["UmbPage"] = path;
						context.Items["VirtualUrl"] = String.Format("{0}{1}", path, query);
						// rewrite to the new URL
						context.RewritePath(string.Format("{0}/default.aspx{2}",
														  SystemDirectories.Root, path, query));
					}
				}
			}
		}


		/// <summary>
		/// Handles the Error event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Application_Error(Object sender, EventArgs e)
		{
			// return immediately if an error is already been handled, to avoid infinite recursion
			if ((bool)handlingError)
			{
				lock (unhandledErrors)
				{
					unhandledErrors.Add(mApp.Server.GetLastError());
				}
				return;
			}
			// make sure only one thread at a time can handle an error
			lock (handlingError)
			{
				Debug.Assert(!(bool)handlingError, "Two errors are being handled at the same time.");
				handlingError = true;

				// make sure handlingError always gets set to false
				try
				{
					if (GlobalSettings.Configured)
					{
						// log the error
						// zb-00017 #29930 : could have been cleared, though: take care, .GetLastError() may return null
						Exception ex = mApp.Server.GetLastError();
						if (ex != null)
							ex = ex.InnerException;

						string error;
						if (mApp.Context.Request != null)
							error = string.Format("At {0} (Referred by: {1}): {2}",
												  mApp.Context.Request.RawUrl,
												  mApp.Context.Request.UrlReferrer,
												  ex);
						else
							error = "No Context available -> "
									+ ex;

						// Hide error if getting the user throws an error (e.g. corrupt / blank db)
						User staticUser = null;
						try
						{
							User.GetCurrent();
						}
						catch
						{
						}

                        LogHelper.Debug<requestModule>(error);

						Trace.TraceError(error);
						lock (unhandledErrors)
						{
							if (unhandledErrors.Count > 0)
								Trace.TraceError("New errors occurred while an error was being handled. The error handler Application_Error possibly raised another error, but was protected against an infinite loop.");
							foreach (Exception unhandledError in unhandledErrors)
								Trace.TraceError(unhandledError.StackTrace);
						}
					}
				}
				finally
				{
					// clear unhandled errors
					lock (unhandledErrors)
					{
						unhandledErrors.Clear();
					}
					// flag we're done with the error handling
					handlingError = false;
				}
			}
		}

		#region IHttpModule Members

		///<summary>
		///Initializes a module and prepares it to handle requests.
		///</summary>
		///
		///<param name="httpContext">An <see cref="T:System.Web.HttpApplication"></see> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
		public void Init(HttpApplication context)
		{
			InitializeComponent();

			ApplicationStart(context);
			context.BeginRequest += new EventHandler(Application_BeginRequest);
			context.AuthorizeRequest += new EventHandler(Application_AuthorizeRequest);

			// Alex Norcliffe - 2010 02 - Changed this behaviour as it disables OutputCaching due to Rewrite happening too early in the chain
			// context.PostAuthorizeRequest += new EventHandler(Application_PostAuthorizeRequest);
			context.PostResolveRequestCache += new EventHandler(Application_PostResolveRequestCache);

			context.PreRequestHandlerExecute += new EventHandler(Application_PreRequestHandlerExecute);

			// Alex Norcliffe - 2010 06 - Added a check at the end of the page lifecycle to see if we should persist Xml cache to disk
			// (a replacement for all those parallel Async methods launching ThreadPool threads)
			context.PostRequestHandlerExecute += new EventHandler(context_PostRequestHandlerExecute);
			context.Error += new EventHandler(Application_Error);
			mApp = context;
		}

		void context_PostRequestHandlerExecute(object sender, EventArgs e)
		{
            //if (content.Instance.IsXmlQueuedForPersistenceToFile)
            //{
            //    content.Instance.PersistXmlToFile();
            //}
		}




		private void InitializeComponent()
		{
			components = new Container();
		}

		///<summary>
		///Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"></see>.
		///</summary>
		///
		public void Dispose()
		{
		}


		//this makes sure that times and other stuff is started on the first request, instead of depending on
		// application_start, which was inteded for being a httpContext-agnostic state.
		private static bool s_InitializedAlready = false;
		private static Object s_lock = new Object();
		// Initialize only on the first request
		public void InitializeApplication(HttpApplication HttpApp)
		{
			if (s_InitializedAlready)
				return;

			lock (s_lock)
			{
				if (s_InitializedAlready)
					return;

				// Perform first-request initialization here ...

				try
				{
                    LogHelper.Info<requestModule>(string.Format("Application started at {0}", DateTime.Now));

                    if (UmbracoConfig.For.UmbracoSettings().Logging.AutoCleanLogs)
					{
						AddTask(LOG_SCRUBBER_TASK_NAME, GetLogScrubbingInterval());
					}
				}
				catch
				{
				}

                // Trigger startup handlers
			    ApplicationStartupHandler.RegisterHandlers();

			    // Check for configured key, checking for currentversion to ensure that a request with
				// no httpcontext don't set the whole app in configure mode
                if (UmbracoVersion.Current != null && !GlobalSettings.Configured)
				{
					HttpApp.Application["umbracoNeedConfiguration"] = true;
				}

				/* This section is needed on start-up because timer objects
				* might initialize before these are initialized without a traditional
				* request, and therefore lacks information on application paths */

				/* Initialize SECTION END */

				// add current default url
				HttpApp.Application["umbracoUrl"] = string.Format("{0}:{1}{2}", HttpApp.Context.Request.ServerVariables["SERVER_NAME"], HttpApp.Context.Request.ServerVariables["SERVER_PORT"], IOHelper.ResolveUrl( SystemDirectories.Umbraco ));

				// Start ping / keepalive timer
				pingTimer = new Timer(new TimerCallback(keepAliveService.PingUmbraco), HttpApp.Context, 60000, 300000);

				// Start publishingservice
				publishingTimer = new Timer(new TimerCallback(publishingService.CheckPublishing), HttpApp.Context, 30000, 60000);

				//Find Applications and event handlers and hook-up the events
				//BusinessLogic.Application.RegisterIApplications();

				//define the base settings for the dependency loader to use the global path settings
				//if (!CompositeDependencyHandler.HandlerFileName.StartsWith(GlobalSettings_Path))
				//    CompositeDependencyHandler.HandlerFileName = GlobalSettings_Path + "/" + CompositeDependencyHandler.HandlerFileName;
				
				// Backwards compatibility - set the path and URL type for ClientDependency 1.5.1 [LK]
				ClientDependency.Core.CompositeFiles.Providers.XmlFileMapper.FileMapVirtualFolder = "~/App_Data/TEMP/ClientDependency";
				ClientDependency.Core.CompositeFiles.Providers.BaseCompositeFileProcessingProvider.UrlTypeDefault = ClientDependency.Core.CompositeFiles.Providers.CompositeUrlType.Base64QueryStrings;

				// init done... 
				s_InitializedAlready = true;

			}

		}

		#endregion

		#region Inteval tasks
		private static int GetLogScrubbingInterval()
		{
			int interval = 24 * 60 * 60; //24 hours
			try
			{
                if (UmbracoConfig.For.UmbracoSettings().Logging.CleaningMiliseconds > -1)
					interval = UmbracoConfig.For.UmbracoSettings().Logging.CleaningMiliseconds;
			}
			catch (Exception)
			{
                LogHelper.Info<requestModule>("Unable to locate a log scrubbing interval. Defaulting to 24 hours");
			}
			return interval;
		}

		private static int GetLogScrubbingMaximumAge()
		{
			int maximumAge = 24 * 60 * 60;
			try
			{
				if (UmbracoConfig.For.UmbracoSettings().Logging.MaxLogAge > -1)
                    maximumAge = UmbracoConfig.For.UmbracoSettings().Logging.MaxLogAge;
			}
			catch (Exception)
			{
                LogHelper.Info<requestModule>("Unable to locate a log scrubbing maximum age. Defaulting to 24 hours");
			}
			return maximumAge;

		}

		private void AddTask(string name, int seconds)
		{
			OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
			HttpRuntime.Cache.Insert(name, seconds, null,
				DateTime.Now.AddSeconds(seconds), System.Web.Caching.Cache.NoSlidingExpiration,
				CacheItemPriority.NotRemovable, OnCacheRemove);
		}

		public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
		{
			if (k.Equals(LOG_SCRUBBER_TASK_NAME))
			{
				ScrubLogs();
			}
			AddTask(k, Convert.ToInt32(v));
		}

		private static void ScrubLogs()
		{
			Log.CleanLogs(GetLogScrubbingMaximumAge());
		}

		#endregion
	}


}