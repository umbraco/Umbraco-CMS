namespace Umbraco.Core
{
    /// <summary>
    /// A place to get/retrieve data in a current context (i.e. http, thread, etc...)
    /// </summary>
    internal interface IScopeContext
    {
        object GetData(string key);
        void SetData(string key, object data);
        void ClearData(string key);
    }
}