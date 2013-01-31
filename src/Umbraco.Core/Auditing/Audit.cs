namespace Umbraco.Core.Auditing
{
	/// <summary>
	/// Public method to create new audit trail
	/// </summary>
    public static class Audit
    {
         public static void Add(AuditTypes type, string comment, int userId, int objectId)
         {
             AuditTrail.Current.AddEntry(type, comment, userId, objectId);
         }
    }
}