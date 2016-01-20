using System;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Used to encapsulate ModelStateDictionary, but because there are 2x this (mvc and webapi), this
    /// is just a simple way to have one object for both
    /// </summary>
    internal class EditorValidationErrors
    {
        private readonly Action<string, string> _addModelError;

        public EditorValidationErrors(Action<string, string> addModelError)
        {
            _addModelError = addModelError;
        }

        public void AddModelError(string key, string message)
        {
            _addModelError(key, message);
        }
    }
}