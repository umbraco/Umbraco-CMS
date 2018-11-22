using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.IO;
using System.Security.AccessControl;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;

namespace umbraco.Utils {
    public class fileHelper {

        public static bool TestReadWriteAccces(string filePath) {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.IsReadOnly)
                return HasAccces(fi, FileSystemRights.Modify);
            else
                return false;
        }

        public static bool HasAccces(FileInfo fileInfo, FileSystemRights fileSystemRights) {
            string identityName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper();

            AuthorizationRuleCollection authorizationRuleCollection = fileInfo.GetAccessControl().GetAccessRules(true, true, typeof(NTAccount));

            foreach (FileSystemAccessRule fileSystemAccessRule in authorizationRuleCollection) {
                if (identityName == fileSystemAccessRule.IdentityReference.Value.ToUpper())
                    return AccessControlType.Allow == fileSystemAccessRule.AccessControlType && fileSystemRights == (fileSystemAccessRule.FileSystemRights & fileSystemRights);
            }

            return false;
        }
    }
}
