using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using System.Linq;

namespace Umbraco.Web.UI
{
    /// <summary>
    /// This is used to replace the old umbraco.presentation.create.dialogHandler_temp class which is used
    /// to handle sections create/delete actions.
    /// </summary>
    /// <remarks>
    /// We need to overhaul how all of this is handled which is why this is a legacy class
    /// http://issues.umbraco.org/issue/U4-1373
    /// </remarks>
    public static class LegacyDialogHandler
    {
        private enum Operation
        {
            Create,
            Delete
        }

        private const string ContextKeyCreate = "LegacyDialogHandler-Create-";
        private const string ContextKeyDelete = "LegacyDialogHandler-Delete-";

        /// <summary>
        /// Gets the ITask for the operation for the node Type
        /// </summary>
        /// <param name="umbracoUser"></param>
        /// <param name="op"></param>
        /// <param name="nodeType"></param>
        /// <param name="httpContext"></param>
        /// <returns>
        /// Returns the ITask if one is found and can be made, otherwise null
        /// </returns>
        /// <remarks>
        /// This will first check if we've already created the ITask in the current Http request
        /// </remarks>
        private static ITask GetTaskForOperation(HttpContextBase httpContext, User umbracoUser, Operation op, string nodeType)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (umbracoUser == null) throw new ArgumentNullException("umbracoUser");
            if (nodeType == null) throw new ArgumentNullException("nodeType");

            var ctxKey = op == Operation.Create ? ContextKeyCreate : ContextKeyDelete;

            //check contextual cache
            if (httpContext.Items[ctxKey] != null)
            {
                return (ITask) httpContext.Items[ctxKey];
            }

            var operationNode = op == Operation.Create ? "create" : "delete";
            var createDef = GetXmlDoc();
            var def = createDef.SelectSingleNode("//nodeType [@alias = '" + nodeType + "']");
            if (def == null)
            {
                return null;
            }
            var del = def.SelectSingleNode("./tasks/" + operationNode);
            if (del == null)
            {
                return null;
            }
            if (!del.Attributes.HasAttribute("assembly"))
            {
                return null;
            }
            var taskAssembly = del.AttributeValue<string>("assembly");
            
            if (!del.Attributes.HasAttribute("type"))
            {
                return null;
            }
            var taskType = del.AttributeValue<string>("type");

            var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + taskAssembly + ".dll"));
            var type = assembly.GetType(taskAssembly + "." + taskType);
            var typeInstance = Activator.CreateInstance(type) as ITask;
            if (typeInstance == null)
            {
                return null;
            }

            //set the user/user id for the instance
            var dialogTask = typeInstance as LegacyDialogTask;
            if (dialogTask != null)
            {
                dialogTask.User = umbracoUser;
            }
            else
            {
                typeInstance.UserId = umbracoUser.Id;
            }

            //put in contextual cache 
            httpContext.Items[ctxKey] = typeInstance;

            return typeInstance;
        }

        /// <summary>
        /// Checks if the user has access to launch the ITask that matches the node type based on the app assigned
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="umbracoUser"></param>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the ITask doesn't implement LegacyDialogTask then we will return 'true' since we cannot validate
        /// the application assigned.
        /// 
        /// TODO: Create an API to assign a nodeType to an app so developers can manually secure it
        /// </remarks>
        internal static bool UserHasCreateAccess(HttpContextBase httpContext, User umbracoUser, string nodeType)
        {
            var task = GetTaskForOperation(httpContext, umbracoUser, Operation.Create, nodeType);
            if (task == null)
            {
                //if no task was found it will use the default task and we cannot validate the application assigned so return true
                return true;
            }

            var dialogTask = task as LegacyDialogTask;
            if (dialogTask != null)
            {
                return dialogTask.ValidateUserForApplication();
            }
            return true;
        }

        /// <summary>
        /// Checks if the user has access to launch the ITask that matches the node type based on the app assigned
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="umbracoUser"></param>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the ITask doesn't implement LegacyDialogTask then we will return 'true' since we cannot validate
        /// the application assigned.
        /// 
        /// TODO: Create an API to assign a nodeType to an app so developers can manually secure it
        /// </remarks>
        internal static bool UserHasDeleteAccess(HttpContextBase httpContext, User umbracoUser, string nodeType)
        {
            var task = GetTaskForOperation(httpContext, umbracoUser, Operation.Delete, nodeType);
            if (task == null)
            {
                //if no task was found it will use the default task and we cannot validate the application assigned so return true
                return true;
            }

            var dialogTask = task as LegacyDialogTask;
            if (dialogTask != null)
            {
                return dialogTask.ValidateUserForApplication();
            }
            return true;
        }

        public static void Delete(HttpContextBase httpContext, User umbracoUser, string nodeType, int nodeId, string text)
        {
            var typeInstance = GetTaskForOperation(httpContext, umbracoUser, Operation.Delete, nodeType);
            if (typeInstance == null)
                throw new InvalidOperationException(
                    string.Format("Could not task for operation {0} for node type {1}", Operation.Delete, nodeType));

            typeInstance.ParentID = nodeId;
            typeInstance.Alias = text;

            typeInstance.Delete();
        }

        public static string Create(HttpContextBase httpContext, User umbracoUser, string nodeType, int nodeId, string text, int typeId = 0)
        {
            var typeInstance = GetTaskForOperation(httpContext, umbracoUser, Operation.Create, nodeType);
            if (typeInstance == null)
                throw new InvalidOperationException(
                    string.Format("Could not task for operation {0} for node type {1}", Operation.Create, nodeType));

            typeInstance.TypeID = typeId;
            typeInstance.ParentID = nodeId;
            typeInstance.Alias = text;

            typeInstance.Save();

            // check for returning url
            var returnUrlTask = typeInstance as ITaskReturnUrl;
            return returnUrlTask != null
                ? returnUrlTask.ReturnUrl
                : "";
        }

        internal static string Create(HttpContextBase httpContext, User umbracoUser, string nodeType, int nodeId, string text, IDictionary<string, object> additionalValues, int typeId = 0)
        {
            var typeInstance = GetTaskForOperation(httpContext, umbracoUser, Operation.Create, nodeType);
            if (typeInstance == null)
                throw new InvalidOperationException(
                    string.Format("Could not task for operation {0} for node type {1}", Operation.Create, nodeType));

            typeInstance.TypeID = typeId;
            typeInstance.ParentID = nodeId;
            typeInstance.Alias = text;

            // check for returning url
            ITaskReturnUrl returnUrlTask = typeInstance as LegacyDialogTask;
            if (returnUrlTask != null)
            {
                // if castable to LegacyDialogTask: add in additionalValues
                ((LegacyDialogTask) returnUrlTask).AdditionalValues = additionalValues;
            }
            else
            {
                // otherwise cast to returnUrl interface
                returnUrlTask = typeInstance as ITaskReturnUrl;
            }
            
            typeInstance.Save();
            
            return returnUrlTask != null
                ? returnUrlTask.ReturnUrl
                : "";
        }

        private static XmlDocument GetXmlDoc()
        {
            // Load task settings
            var createDef = new XmlDocument();
            using (var defReader = new XmlTextReader(IOHelper.MapPath(SystemFiles.CreateUiXml)))
            {
                createDef.Load(defReader);
                defReader.Close();
                return createDef;
            }            
        }
    }
}
