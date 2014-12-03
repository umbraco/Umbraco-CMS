using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Editors.Filters;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// This filter is used on all PostSave methods for Content, Media and Members. It's purpose is to instantiate a new instance of
    /// ContentItemValidationHelper{TPersisted, TModelSave} which is used to validate the content properties of these entity types.
    /// This filter is then executed after the model is bound but before the action is executed.
    /// </summary>    
    internal sealed class ContentModelValidationFilter : ActionFilterAttribute
    {
        private readonly Type _customValidationHelperType;
        private readonly Type _contentItemSaveType;
        private readonly Type _contentPersistedType;

        private static readonly ConcurrentDictionary<Tuple<Type, Type>, ValidationHelperReflectedType> ReflectionCache = new ConcurrentDictionary<Tuple<Type, Type>, ValidationHelperReflectedType>();

        /// <summary>
        /// Constructor accepting a custom instance of a ContentItemValidationHelper{TPersisted, TModelSave}
        /// </summary>
        /// <param name="customValidationHelperType"></param>
        public ContentModelValidationFilter(Type customValidationHelperType)
        {
            if (customValidationHelperType == null) throw new ArgumentNullException("customValidationHelperType");
            _customValidationHelperType = customValidationHelperType;
        }

        /// <summary>
        /// Constructor accepting the types required to create an instance of the desired ContentItemValidationHelper{TPersisted, TModelSave}
        /// </summary>
        /// <param name="contentItemSaveType"></param>
        /// <param name="contentPersistedType"></param>
        public ContentModelValidationFilter(Type contentItemSaveType, Type contentPersistedType)
        {
            if (contentItemSaveType == null) 
                throw new ArgumentNullException("contentItemSaveType");
            if (contentPersistedType == null) 
                throw new ArgumentNullException("contentPersistedType");
            if (TypeHelper.IsTypeAssignableFrom<ContentItemBasic>(contentItemSaveType) == false) 
                throw new ArgumentException("Invalid base type", "contentItemSaveType");
            if (TypeHelper.IsTypeAssignableFrom<IContentBase>(contentPersistedType) == false)
                throw new ArgumentException("Invalid base type", "contentPersistedType");
            
            _contentItemSaveType = contentItemSaveType;
            _contentPersistedType = contentPersistedType;
        }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var contentItem = actionContext.ActionArguments["contentItem"];

            //NOTE: This will be an instance of ContentItemValidationHelper<TPersisted, TModelSave>
            object validator;
            MethodInfo validateMethod;
            if (_customValidationHelperType != null)
            {
                //Get the validator for this generic type
                validator = Activator.CreateInstance(_customValidationHelperType);
                validateMethod = _customValidationHelperType.GetMethod("ValidateItem");
            }
            else
            {
                var reflectedInfo = ReflectionCache.GetOrAdd(new Tuple<Type, Type>(_contentItemSaveType, _contentPersistedType),
                tuple =>
                {
                    var validationHelperGenericType = typeof(ContentValidationHelper<,>);
                    var realType = validationHelperGenericType.MakeGenericType(_contentPersistedType, _contentItemSaveType);
                    return new ValidationHelperReflectedType
                    {
                        RealType = realType,
                        ValidateMethod = realType.GetMethod("ValidateItem")
                    };
                });

                //Get the validator for this generic type
                validator = Activator.CreateInstance(reflectedInfo.RealType);

                validateMethod = reflectedInfo.ValidateMethod;
            }

            
            //Now call the methods for this instance
            validateMethod.Invoke(validator, new object[] { actionContext, contentItem });

        }

        private class ValidationHelperReflectedType
        {
            public Type RealType { get; set; }
            public MethodInfo ValidateMethod { get; set; }
        }
    }
}