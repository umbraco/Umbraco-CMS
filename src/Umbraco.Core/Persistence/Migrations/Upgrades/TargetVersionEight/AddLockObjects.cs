using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight
{
    [Migration("8.0.0", 100, GlobalSettings.UmbracoMigrationName)]
    public class AddLockObjects : MigrationBase
    {
        public AddLockObjects(ILogger logger)
            : base(logger)
        { }

        public override void Up()
        {
            // that one should exist already but may be missing in some cases
            EnsureLockObject(Constants.System.ServersLock, "0AF5E610-A310-4B6F-925F-E928D5416AF7", "LOCK: Servers");

            // those are new
            EnsureLockObject(Constants.System.ContentTypesLock, "A88CE058-A627-46BB-878A-07C5FE3D870A", "LOCK: ContentTypes");
            EnsureLockObject(Constants.System.ContentTreeLock, "3FB9211A-B4F9-449C-8725-A075AE500518", "LOCK: ContentTree");
            EnsureLockObject(Constants.System.MediaTreeLock, "5B4408F9-D9FB-4145-84BB-5F0F2C35B4B0", "LOCK: MediaTree");
            EnsureLockObject(Constants.System.MemberTreeLock, "FA951390-DF12-4594-8366-89CA8396D977", "LOCK: MemberTree");
            EnsureLockObject(Constants.System.MediaTypesLock, "BA13E02F-E595-4415-8693-B044C83AA9A7", "LOCK: MediaTypes");
            EnsureLockObject(Constants.System.MemberTypesLock, "B23EED6B-CC05-48FE-B096-B50441D0E825", "LOCK: MemberTypes");
            EnsureLockObject(Constants.System.DomainsLock, "FE7F0984-6BC9-440E-B105-4B7206B3F6E2", "LOCK: Domains");
        }

        public override void Down()
        {
            // not implemented
        }

        private void EnsureLockObject(int id, string uniqueId, string text)
        {
            var exists = Context.Database.Exists<NodeDto>(id);
            if (exists) return;

            Insert
                .IntoTable("umbracoNode")
                .EnableIdentityInsert()
                .Row(new
                {
                    id = id, // NodeId
                    trashed = false,
                    parentId = -1,
                    nodeUser = 0,
                    level = 1,
                    path = "-1," + id,
                    sortOrder = 0,
                    uniqueId = new Guid(uniqueId),
                    text = text,
                    nodeObjectType = Constants.ObjectTypes.LockObject,
                    createDate = DateTime.Now
                });
        }
    }
}
