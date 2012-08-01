using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;
using umbraco.cms;
using System.IO;

namespace umbraco.webservices.files
{
    /// <summary>
    /// Summary description for FileService
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class fileService : BaseWebService
    {

        override public Services Service
        {
            get
            {
                return Services.FileService;
            }
        }

        [WebMethod]
        public void DeleteFile(String folderName, string fileName, string username, string password)
        {
            
            Authenticate(username, password);
            
            // Check if folder is accessible
            if (FileIO.FolderAccess(folderName))
            {   
                // Check if the filename is valid
                if (!FileIO.ValidFileName(fileName))
                    throw new ArgumentException(String.Format("Filename {0} not valid", fileName));

                 // Check if the file exists. If it does, we delete it
                if (System.IO.File.Exists(FileIO.GetFilePath(folderName, fileName)))
                {
                    System.IO.File.Delete(FileIO.GetFilePath(folderName, fileName));
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            else
            {
                throw new ArgumentException("no access to specified folder");
            }
        }
        [WebMethod]
        public Byte[] DownloadFile(String folderName, string fileName, string username, string password)
        {

            Authenticate(username, password);

            // Check if folder is accessible
            if (FileIO.FolderAccess(folderName))
            {
                // Check if the filename is valid
                if (!FileIO.ValidFileName(fileName))
                    throw new ArgumentException(String.Format("Filename {0} not valid", fileName));

                // Check if the file even exists
                if (!System.IO.File.Exists(FileIO.GetFilePath(folderName, fileName)))
                {
                    throw new FileNotFoundException("Could not find file to delete");
                }

                // Create a stream, and send it to the client
                FileStream objfilestream = new FileStream(FileIO.GetFilePath(folderName, fileName), FileMode.Open, FileAccess.Read);
                int len = (int)objfilestream.Length;
                Byte[] documentcontents = new Byte[len];
                objfilestream.Read(documentcontents, 0, len);
                objfilestream.Close();
                return documentcontents;
            }
            else
            {
                throw new ArgumentException("no access to specified folder");
            }
        }

        [WebMethod]
        public void UploadFile(Byte[] docbinaryarray, String folderName, string fileName, string username, string password, bool deleteOld)
        {
            Authenticate( username,  password);

            // Check if folder is accessible
            if (FileIO.FolderAccess(folderName))
            {
                // Check if the filename is valid
                if (FileIO.ValidFileName(fileName))
                    throw new ArgumentException(String.Format("Filename {0} not valid", fileName));

                // Check if the file exists. If it does, we delete it first .. 
                // TODO: Maybe we should have "deleted files, folder for this?
                if (System.IO.File.Exists(FileIO.GetFilePath(folderName, fileName)))
                {
                    if (deleteOld)
                    {
                        System.IO.File.Delete(FileIO.GetFilePath(folderName, fileName));
                    }
                    else
                    {
                        throw new ArgumentException("Cannot save. File allready exist");
                    }
                }

                // Open a filestream, and write the data from the client to it
                FileStream objfilestream = new FileStream(FileIO.GetFilePath(folderName, fileName), FileMode.Create, FileAccess.ReadWrite);
                objfilestream.Write(docbinaryarray, 0, docbinaryarray.Length);
                objfilestream.Close();
            }
            else
            {
                throw new ArgumentException("no access to specified folder");
            }
        }


        /// <summary>
        /// To download a file, we need to know how big its going to be
        /// </summary>
        [WebMethod]
        public int GetFileSize(String folderName, string fileName, string username, string password)
        {
            Authenticate(username, password);

            // Check if folder is accessible
            if (FileIO.FolderAccess(folderName))
            {

                // Check if the filename is valid
                if (!FileIO.ValidFileName(fileName))
                    throw new ArgumentException(String.Format("Filename {0} not valid", fileName));

                string strdocPath;
                strdocPath = FileIO.GetFilePath(folderName, fileName);

                // Load file into stream
                FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);

                // Find and return the lenght of the stream
                int len = (int)objfilestream.Length;
                objfilestream.Close();
                return len;
            }
            else
            {
                throw new ArgumentException("no access to specified folder");
            }
        }


        /// <summary>
        /// Get all files in a specific folder
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string[] GetFilesList(String folderName, string username, string password)
        {
            Authenticate(username, password);

            if (FileIO.FolderAccess(folderName))
            {
                string fullPath = FileIO.GetFolderPath(folderName);
                                
                DirectoryInfo folder = new DirectoryInfo(fullPath);
                FileInfo[] files = folder.GetFiles();
                List<string> shortNames = new List<string> (  );

                foreach (FileInfo file in files)
                {
                    shortNames.Add(file.Name);
                }
                return shortNames.ToArray();

            }
            else
            {
                throw new ArgumentException("no access to specified folder");
            }
        }

    }
}
