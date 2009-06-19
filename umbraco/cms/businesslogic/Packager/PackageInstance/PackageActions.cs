using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Xml;

using umbraco.BasePages;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.workflow;
using umbraco.interfaces;


namespace umbraco.cms.businesslogic.packager {

    /// <summary>
    /// Package actions are executed on packge install / uninstall.
    /// </summary>
    public class PackageAction {
        private static readonly List<IPackageAction> _packageActions = new List<IPackageAction>();

        /// <summary>
        /// Initializes the <see cref="PackageAction"/> class.
        /// </summary>
        static PackageAction(){
            RegisterPackageActions();
        }
        
        private static void RegisterPackageActions()
        {
            List<Type> types = TypeFinder.FindClassesOfType<IPackageAction>(true);
            foreach (Type t in types)
            {
                IPackageAction typeInstance = Activator.CreateInstance(t) as IPackageAction;
                if (typeInstance != null)
                {
                    _packageActions.Add(typeInstance);
                                     
                    if (HttpContext.Current != null)
                        HttpContext.Current.Trace.Write("registerPackageActions", " + Adding package action '" + typeInstance.Alias());
                }                   
            }
        }

        /// <summary>
        /// Runs the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public static void RunPackageAction(string packageName, string actionAlias, System.Xml.XmlNode actionXml) {

            foreach (IPackageAction ipa in _packageActions) {
                try {
                    if (ipa.Alias() == actionAlias) {

                        ipa.Execute(packageName, actionXml);
                    }
                } catch (Exception ipaExp) {
                    BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, BusinessLogic.User.GetUser(0), -1, string.Format("Error loading package action '{0}' for package {1}: {2}",
                        ipa.Alias(), packageName, ipaExp));
                }
            }
        }

        /// <summary>
        /// Undos the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public static void UndoPackageAction(string packageName, string actionAlias, System.Xml.XmlNode actionXml) {

            foreach (IPackageAction ipa in _packageActions) {
                try {
                    if (ipa.Alias() == actionAlias) {

                        ipa.Undo(packageName, actionXml);
                    }
                } catch (Exception ipaExp) {
                    BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, BusinessLogic.User.GetUser(0), -1, string.Format("Error undoing package action '{0}' for package {1}: {2}",
                        ipa.Alias(), packageName, ipaExp));
                }
            }
        }
        
    }
}
