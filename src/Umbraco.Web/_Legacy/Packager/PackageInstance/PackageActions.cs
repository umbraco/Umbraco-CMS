using System;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core._Legacy.PackageActions;


namespace umbraco.cms.businesslogic.packager
{

    /// <summary>
    /// Package actions are executed on packge install / uninstall.
    /// </summary>
    public class PackageAction
    {

        /// <summary>
        /// Runs the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public static void RunPackageAction(string packageName, string actionAlias, XmlNode actionXml)
        {

            foreach (var ipa in Current.PackageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                    {

                        ipa.Execute(packageName, actionXml);
                    }
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<PackageAction>(ex, "Error loading package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
                }
            }
        }

        /// <summary>
        /// Undos the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public static void UndoPackageAction(string packageName, string actionAlias, System.Xml.XmlNode actionXml)
        {

            foreach (IPackageAction ipa in Current.PackageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                    {

                        ipa.Undo(packageName, actionXml);
                    }
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<PackageAction>(ex, "Error undoing package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
                }
            }
        }

    }
}
