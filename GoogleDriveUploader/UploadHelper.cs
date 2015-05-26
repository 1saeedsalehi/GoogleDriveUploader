using System;
using System.Collections.Generic;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;

namespace GoogleDriveUploader
{
    public class UploadHelper
    {


        /// <summary>
        /// Update an existing file's metadata and content.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <param name="fileId">ID of the file to update.</param>
        /// <param name="newTitle">New title for the file.</param>
        /// <param name="newDescription">New description for the file.</param>
        /// <param name="newMimeType">New MIME type for the file.</param>
        /// <param name="newFilename">Filename of the new content to upload.</param>
        /// <param name="newRevision">Whether or not to create a new revision for this file.</param>
        /// <returns>Updated file metadata, null is returned if an API error occurred.</returns>
        public static File updateFile(DriveService service, String fileId, String newTitle,
            String newDescription, String newMimeType, String newFilename, bool newRevision)
        {
            try
            {
                // First retrieve the file from the API.
                File file = service.Files.Get(fileId).Execute();

                // File's new metadata.
                file.Title = newTitle;
                file.Description = newDescription;
                file.MimeType = newMimeType;

                // File's new content.
                byte[] byteArray = System.IO.File.ReadAllBytes(newFilename);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                // Send the request to the API.
                FilesResource.UpdateMediaUpload request = service.Files.Update(file, fileId, stream, newMimeType);
                request.NewRevision = newRevision;
                request.Upload();

                File updatedFile = request.ResponseBody;
                return updatedFile;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }
        /**
         * Permanently delete a file, skipping the trash.
         *
         * @param service Drive API service instance.
         * @param fileId ID of the file to delete.
         */
        public static void deleteFile(DriveService service, String fileId)
        {
            try
            {
                service.Files.Delete(fileId).Execute();
            }
            catch (System.IO.IOException e)
            {

            }
        }


        /// <summary>
        /// Retrieve a list of File resources.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <returns>List of File resources.</returns>
        public static List<File> retrieveAllFiles(DriveService service)
        {
            List<File> result = new List<File>();
            FilesResource.ListRequest request = service.Files.List();

            do
            {
                try
                {
                    FileList files = request.Execute();

                    result.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));
            return result;
        }

        /// <summary>
        /// Move a file to the trash.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <param name="fileId">ID of the file to trash.</param>
        /// <returns>The updated file, null is returned if an API error occurred</returns>
        public static File TrashFile(DriveService service, String fileId)
        {
            try
            {
                return service.Files.Trash(fileId).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }


        public static File UpdateFile(DriveService service, string uploadFile, string parent, string fileId)
        {

            if (System.IO.File.Exists(uploadFile))
            {
                var body = new File
                {
                    Title = System.IO.Path.GetFileName(uploadFile),
                    Description = "File updated by DriveUploader for Windows",
                    MimeType = GetMimeType(uploadFile),
                    Parents = new List<ParentReference>()
                              {
                                  new ParentReference()
                                  {
                                      Id = parent
                                  }
                              }
                };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
                var stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.UpdateMediaUpload request = service.Files.Update(body, fileId, stream, GetMimeType(uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + uploadFile);
                return null;
            }

        }

        public static File CreateDirectory(DriveService service, String directoryTitle, String directoryDescription = "Backup of files")
        {

            File newDirectory = null;

            var body = new File
                       {
                           Title = directoryTitle,
                           Description = directoryDescription,
                           MimeType = "application/vnd.google-apps.folder",
                           Parents = new List<ParentReference>()
                                     {
                                         new ParentReference()
                                         {
                                             Id = "root"
                                         }
                                     }
                       };
            try
            {
                var request = service.Files.Insert(body);
                newDirectory = request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return newDirectory;
        }

        public static string GetMimeType(string fileName)
        {
            var mimeType = "application/unknown";
            var extension = System.IO.Path.GetExtension(fileName);

            if (extension == null)
            {
                return mimeType;
            }

            var ext = extension.ToLower();
            var regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

            if (regKey != null && regKey.GetValue("Content Type") != null)
            {
                mimeType = regKey.GetValue("Content Type").ToString();
            }

            return mimeType;
        }

        public static File UploadFile(DriveService service, string uploadFile, string parent)
        {
            if (System.IO.File.Exists(uploadFile))
            {
                var body = new File
                           {
                               Title = System.IO.Path.GetFileName(uploadFile),
                               Description = "File uploaded by DriveUploader For Windows",
                               MimeType = GetMimeType(uploadFile),
                               Parents = new List<ParentReference>()
                                         {
                                             new ParentReference()
                                             {
                                                 Id = parent
                                             }
                                         }
                           };

                byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
                var stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, GetMimeType(uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + uploadFile);
                return null;
            }

        }
    }
}
