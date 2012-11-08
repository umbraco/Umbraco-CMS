using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.PartialTrust
{
	public static class PartialTrustHelper<T>
		where T : class, new()
	{
		public const string PartialTrustAppDomainName = "Partial Trust AppDomain";

		public static void RunInPartial(MethodInfo methodInfo)
		{
			AppDomain partiallyTrustedDomain = CreatePartialTrustDomain();

			try
			{
				RunInPartial(methodInfo, partiallyTrustedDomain);
			}
			finally
			{
				AppDomain.Unload(partiallyTrustedDomain);
			}
		}

		/// <summary>
		/// Runs the provided <paramref name="methodInfo"/> in the <paramref name="partiallyTrustedDomain"/>.
		/// If <paramref name="marshaller"/> is provided, it will be used, otherwise a new one will be created.
		/// </summary>
		/// <param name="methodInfo">The method info.</param>
		/// <param name="partiallyTrustedDomain">The partially trusted domain.</param>
		/// <param name="marshaller">The marshaller.</param>
		public static void RunInPartial(MethodInfo methodInfo, AppDomain partiallyTrustedDomain, PartialTrustMethodRunner<T> marshaller = null)
		{
			// Ensure no mistakes creep in and this code is actually running on the fully trusted AppDomain
			Assert.That(AppDomain.CurrentDomain.FriendlyName, Is.Not.StringStarting(PartialTrustAppDomainName));

			if (marshaller == null) marshaller = GenerateMarshaller(partiallyTrustedDomain);

			try
			{
				marshaller.Run(methodInfo);
			}
			catch (PartialTrustTestException ex)
			{
				throw ex;
			}
			catch (TargetInvocationException ex)
			{
				// If this gets raised, it could be because the method failed, or an assertion was called (either pass or failure)
				if (ex.InnerException != null)
				{
					if (ex.InnerException is SuccessException)
						/* Do nothing but return - it's NUnit's way of exiting out of a method when Assert.Pass() is called */
						return;
					Assert.Fail(ex.InnerException.ToString());
				}
				Assert.Fail("Failed but InnerException was null; " + ex.Message);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.ToString());
			}
		}

		/// <summary>
		/// Generates a marshaller proxy of <see cref="PartialTrustMethodRunner{TFixture}"/> inside the provided <paramref name="partiallyTrustedDomain"/>.
		/// </summary>
		/// <param name="partiallyTrustedDomain">The partially trusted domain.</param>
		/// <returns></returns>
		public static PartialTrustMethodRunner<T> GenerateMarshaller(AppDomain partiallyTrustedDomain)
		{
			var marshallerType = typeof(PartialTrustMethodRunner<T>);
			var marshaller =
				partiallyTrustedDomain.CreateInstanceAndUnwrap(marshallerType.Assembly.FullName, marshallerType.FullName) as
				PartialTrustMethodRunner<T>;
			return marshaller;
		}

		/// <summary>
		/// Creates the an <see cref="AppDomain"/> with a partial trust <see cref="PermissionSet"/>.
		/// </summary>
		/// <returns></returns>
		public static AppDomain CreatePartialTrustDomain()
		{
			var setup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
			var permissions = MediumTrustConfigHelper.GetMediumTrustPermissionSet();
			return AppDomain.CreateDomain("Partial Trust AppDomain: " + DateTime.Now.Ticks, null, setup, permissions);
		}

		public static void CheckRunNUnitTestInPartialTrust()
		{
			var findMethodInfo = FindCurrentTestMethodInfo();

			try
			{
				RunInPartial(findMethodInfo);
			}
			catch (SuccessException assertPass)
			{
				/* This means the test passed with a direct call to Pass, so do nothing */
				throw;
			}
		}

		public static MethodInfo FindCurrentTestMethodInfo()
		{
			var nameOfTest = TestContext.CurrentContext.Test.Name;
			var findMethodInfo = typeof(T).GetMethod(nameOfTest);
			return findMethodInfo;
		}

		public static void CheckRunNUnitTestInPartialTrust(AppDomain withAppDomain, PartialTrustMethodRunner<T> marshaller)
		{
			var findMethodInfo = FindCurrentTestMethodInfo();

			try
			{
				RunInPartial(findMethodInfo, withAppDomain, marshaller);
			}
			catch (SuccessException assertPass)
			{
				/* This means the test passed with a direct call to Pass, so do nothing */
				throw;
			}
		}

		[Serializable]
		public class PartialTrustMethodRunner<TFixture> : MarshalByRefObject
			where TFixture : class, new()
		{
			protected TFixture Instance { get; set; }
			public Type TestClass { get { return typeof(TFixture); } }

			public void Run(MethodInfo methodToRun)
			{
				// Verify that we are definitely running in an appdomain that we made ourselves
				Assert.That(AppDomain.CurrentDomain.FriendlyName, Is.StringStarting(PartialTrustAppDomainName));

				try
				{
					// Support having the marshaller run multiple methods on the same instance by storing the instance locally
					if (Instance == null) Instance = new TFixture();

					// Run the method on the instance we've either created or cached locally
					methodToRun.Invoke(Instance, null);
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null && (ex.InnerException is SecurityException || ex.InnerException is MemberAccessException))
					{
						var inner = ex.InnerException;
						throw new PartialTrustTestException("Test {0} fails in a partial trust environment, due to: {1}".InvariantFormat(methodToRun.Name, inner.Message), ex.InnerException);
					}
					throw;
				}
			}
		}

		public static class MediumTrustConfigHelper
		{
			/// <summary>
			/// Gets the medium trust permission set from the default config path.
			/// </summary>
			/// <returns></returns>
			public static NamedPermissionSet GetMediumTrustPermissionSet()
			{
				return GetMediumTrustPermissionSet(GetMediumTrustConfigPath());
			}

			/// <summary>
			/// Gets the medium trust permission set.
			/// </summary>
			/// <param name="pathToConfigFile">The path to the config file.</param>
			/// <returns></returns>
			public static NamedPermissionSet GetMediumTrustPermissionSet(string pathToConfigFile)
			{
				// Load the config file trusting that it exists.
				var xDocument = XDocument.Load(pathToConfigFile);

				// Get all of the SecurityClass elements which we'll use later to look
				// up a type strongname given a key
				var securityClasses = xDocument.Descendants("SecurityClass").Select(
					x => new
						{
							Name = (string)x.Attribute("Name"),
							Type = (string)x.Attribute("Description")
						});

				// Get the first PermissionSet element where the Name attribute is "ASP.Net"
				var namedSet = xDocument.Descendants("PermissionSet").Where(x => (string)x.Attribute("Name") == "ASP.Net").FirstOrDefault();

				// If we didn't find it, that's a fail
				Assert.NotNull(namedSet);

				// Create a new SecurityElement class to mimic what is represented in Xml
				var secElement = new SecurityElement("PermissionSet");
				secElement.AddAttribute("Name", "ASP.Net");
				secElement.AddAttribute("class", "NamedPermissionSet");
				secElement.AddAttribute("version", "1");

				// For each child of the ASP.Net PermissionSet, create a child SecurityElement representing the IPermission
				foreach (var xElement in namedSet.Elements())
				{
					var child = new SecurityElement("IPermission");

					// Check if we need to do any string replacement on the Xml values first
					ProcessIPermissionAttributeValue(xElement);

					// Get the attributes of the IPermission from Xml and put them onto our child SecurityElement
					foreach (var xAttribute in xElement.Attributes())
					{
						var attribName = xAttribute.Name.LocalName;
						var value = xAttribute.Value;

						try
						{
							if (attribName == "class")
								// This is the type key. Get the full type name from the SecurityClasses list we grabbed earlier
								value = securityClasses.Where(x => x.Name == value).Select(x => x.Type).Single();
						}
						catch (Exception ex)
						{
							throw new XmlException("Could not find the fully-qualified type name for " + value, ex);
						}

						child.AddAttribute(attribName, value);
					}
					secElement.AddChild(child);
				}

				// Create a new NamedPermissionSet, pass in the SecurityElement class representing the Xml
				var permissionSet = new NamedPermissionSet("ASP.Net");
				permissionSet.FromXml(secElement);
				return permissionSet;
			}

			/// <summary>
			/// Processes the custom attribute values of IPermission config elements to replicate what ASP.Net performs
			/// when scanning the permission set. Primarily just replaces $AppDir$ on a FileIOPermission with the current
			/// directory.
			/// </summary>
			/// <param name="element">The element.</param>
			public static void ProcessIPermissionAttributeValue(XElement element)
			{
				var classKey = (string)element.Attribute("class");
				if (classKey == null) return;

				// These attributes are on every IPermission, we can ignore them later
				var ignoredAttributes = new[] { "class", "version" };

				// Get all the attribute on this IPermission that are "custom"
				var customAttributes = element.Attributes().Where(x => !ignoredAttributes.Contains(x.Name.LocalName));
				foreach (var customAttribute in customAttributes)
				{
					switch (classKey)
					{
						case "FileIOPermission":
							switch (customAttribute.Name.LocalName)
							{
								case "Read":
								case "Write":
								case "Append":
								case "PathDiscovery":
									// Replace an $AppDir$ token with the current directory to mimic setting it to the IIS root
									customAttribute.Value = customAttribute.Value.Replace("$AppDir$", Environment.CurrentDirectory);
									break;
							}
							break;
					}
				}
			}

			/// <summary>
			/// Gets the medium trust config path for the current runtime environment.
			/// </summary>
			/// <returns></returns>
			public static string GetMediumTrustConfigPath()
			{
				var readFromDirectory = Path.GetDirectoryName(RuntimeEnvironment.SystemConfigurationFile);
				var autoPath = Path.Combine(readFromDirectory, "web_mediumtrust.config");
				return autoPath;
			}
		}
	}
}