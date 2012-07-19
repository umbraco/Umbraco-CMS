namespace Umbraco.Web.Routing
{


    internal interface ILookup
    {
        bool LookupDocument(DocumentRequest docRequest);
    }
}