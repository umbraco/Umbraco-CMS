namespace Umbraco.Web.Routing
{
    internal interface ILookupNotFound
    {
        bool LookupDocument(DocumentRequest docRequest);
    }
}