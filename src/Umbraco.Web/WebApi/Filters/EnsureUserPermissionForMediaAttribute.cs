﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Auth filter to check if the current user has access to the content item
    /// </summary>
    /// <remarks>
    /// Since media doesn't have permissions, this simply checks start node access    
    /// </remarks>
    internal sealed class EnsureUserPermissionForMediaAttribute : ActionFilterAttribute
    {
        private readonly int? _nodeId;
        private readonly string _paramName;

        public enum DictionarySource
        {
            ActionArguments,
            RequestForm,
            RequestQueryString
        }

        /// <summary>
        /// This constructor will only be able to test the start node access
        /// </summary>
        public EnsureUserPermissionForMediaAttribute(int nodeId)
        {
            _nodeId = nodeId;
        }

        public EnsureUserPermissionForMediaAttribute(string paramName)
        {
            Mandate.ParameterNotNullOrEmpty(paramName, "paramName");
            _paramName = paramName;
        }

        public EnsureUserPermissionForMediaAttribute(string paramName, DictionarySource source)
        {
            Mandate.ParameterNotNullOrEmpty(paramName, "paramName");
            _paramName = paramName;
        }
       
        public override bool AllowMultiple
        {
            get { return true; }
        }

        private int GetNodeIdFromParameter(object parameterValue)
        {
            if (parameterValue is int)
            {
                return (int) parameterValue;
            }

            var guidId = Guid.Empty;
            if (parameterValue is Guid)
            {
                guidId = (Guid)parameterValue;
            }
            else if (parameterValue is GuidUdi)
            {
                guidId = ((GuidUdi) parameterValue).Guid;
            }

            if (guidId != Guid.Empty)
            {
                var found =  ApplicationContext.Current.Services.EntityService.GetIdForKey(guidId, UmbracoObjectTypes.Media);
                if (found)
                    return found.Result;
            }

            throw new InvalidOperationException("The id type: " + parameterValue.GetType() + " is not a supported id");
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (UmbracoContext.Current.Security.CurrentUser == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            int nodeId;
            if (_nodeId.HasValue == false)
            {
                var parts = _paramName.Split(new [] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (actionContext.ActionArguments[parts[0]] == null)
                {
                    throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                }

                if (parts.Length == 1)
                {
                    nodeId = GetNodeIdFromParameter(actionContext.ActionArguments[parts[0]]);
                }
                else
                {
                    //now we need to see if we can get the property of whatever object it is
                    var pType = actionContext.ActionArguments[parts[0]].GetType();
                    var prop = pType.GetProperty(parts[1]);
                    if (prop == null)
                    {
                        throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                    }
                    nodeId = GetNodeIdFromParameter(prop.GetValue(actionContext.ActionArguments[parts[0]]));
                }
            }
            else
            {
                nodeId = _nodeId.Value;
            }

            if (MediaController.CheckPermissions(
                actionContext.Request.Properties,
                UmbracoContext.Current.Security.CurrentUser, 
                ApplicationContext.Current.Services.MediaService, 
                ApplicationContext.Current.Services.EntityService, 
                nodeId))
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }
            
        }
        
        

    }
}