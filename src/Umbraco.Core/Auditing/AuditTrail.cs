using System;

namespace Umbraco.Core.Auditing
{
    /// <summary>
    /// Represents the Audit implementation
    /// </summary>
    internal class AuditTrail
    {
        #region Singleton

        private static readonly Lazy<AuditTrail> lazy = new Lazy<AuditTrail>(() => new AuditTrail());

        public static AuditTrail Current { get { return lazy.Value; } }

        private AuditTrail()
        {
            WriteProvider = new DataAuditWriteProvider();
        }

        #endregion

        private IAuditWriteProvider WriteProvider { get; set; }

        public void AddEntry(AuditTypes type, string comment, int userId, int objectId)
        {
            WriteProvider.WriteEntry(objectId, userId, DateTime.UtcNow, type.ToString(), comment);
        }
    }
}