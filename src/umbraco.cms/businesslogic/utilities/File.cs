using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.cms.businesslogic.utilities {
    public class IOPermissions {

        public static bool IsFileWriteable(string path) {
            if(System.IO.File.Exists(path)){
                System.IO.FileInfo fi = new System.IO.FileInfo(path);
                fi.OpenWrite();
            }

            return false;
        }



    }
}
