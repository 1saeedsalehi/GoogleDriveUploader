using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NLog;


namespace GoogleDriveUploader
{
    public class UploadHelper : IUploadHelper
    {
        private string DirectoryId { set; get; }
        private DriveService Service { set; get; }
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private String folder;
        private String clientId;
        private String clientSecret;
        private String applicationName;
        private String folderName;

        public UploadHelper(String folder, String clientId, String clientSecret, String applicationName, String folderName)
        {
            this.folder = folder;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.applicationName = applicationName;
            this.folderName = folderName;
            ConnectToGoogleDriveServiceAsyn();
        }
        public void ConnectToGoogleDriveServiceAsyn()
        {
            Task.Factory.StartNew(() => { ConnectToGoogleDriveService(folder, clientId, clientSecret, applicationName, folderName); });
        }
        private void ConnectToGoogleDriveService(string folder, string clientId, string clientSecret, string applicationName,
                                                     string folderName)
        {
            var scopes = new[]
                {
                    DriveService.Scope.Drive,
                    DriveService.Scope.DriveFile
                };

            var dataStore = new FileDataStore(folder);

            var secrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret };

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets,
                                                                         scopes,
                                                                         "user",
                                                                         CancellationToken.None,
                                                                         dataStore).Result;

            var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName,
                });
            this.Service = service;

            var query = string.Format("title = '{0}' and mimeType = 'application/vnd.google-apps.folder'", folderName);

            var files = FileHelper.GetFiles(service, query);

            // If there isn't a directory with this name lets create one.
            if (files.Count == 0)
            {
                files.Add(this.CreateDirectory(folderName));
            }

            if (files.Count != 0)
            {
                string directoryId = files[0].Id;
                this.DirectoryId = directoryId;


                // File newFile = UploadHelper.UploadFile(service, @"c:\temp\Lighthouse.jpg", directoryId);

                // File updatedFile = UploadHelper.UpdateFile(service, @"c:\temp\Lighthouse.jpg", directoryId, newFile.Id);
            }
        }

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
        public File updateFile(String fileId, String newTitle,
            String newDescription, String newMimeType, String newFilename, bool newRevision)
        {
            try
            {
                // First retrieve the file from the API.
                File file = Service.Files.Get(fileId).Execute();

                // File's new metadata.
                file.Title = newTitle;
                file.Description = newDescription;
                file.MimeType = newMimeType;

                // File's new content.
                byte[] byteArray = System.IO.File.ReadAllBytes(newFilename);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                // Send the request to the API.
                FilesResource.UpdateMediaUpload request = Service.Files.Update(file, fileId, stream, newMimeType);
                request.NewRevision = newRevision;
                request.Upload();

                File updatedFile = request.ResponseBody;
                return updatedFile;
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred: " + e.Message, e);
                return null;
            }
        }
        /**
         * Permanently delete a file, skipping the trash.
         *
         * @param service Drive API service instance.
         * @param fileId ID of the file to delete.
         */
        public void deleteFile(String fileId)
        {
            try
            {
                Service.Files.Delete(fileId).Execute();
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
        public List<File> retrieveAllFiles()
        {
            List<File> result = new List<File>();
            FilesResource.ListRequest request = Service.Files.List();

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
                    Logger.Error("An error occurred: " + e.Message, e);
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
        public File TrashFile(String fileId)
        {
            try
            {
                return Service.Files.Trash(fileId).Execute();
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred: " + e.Message, e);
            }
            return null;
        }

        /// <summary>
        /// Insert new file.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <param name="title">Title of the file to insert, including the extension.</param>
        /// <param name="description">Description of the file to insert.</param>
        /// <param name="parentId">Parent folder's ID.</param>
        /// <param name="mimeType">MIME type of the file to insert.</param>
        /// <param name="filename">Filename of the file to insert.</param><br>  /// <returns>Inserted file metadata, null is returned if an API error occurred.</returns>
        public File insertFile(String title, String description, String mimeType, String filename)
        {
            // File's metadata.
            File body = new File();
            body.Title = title;
            body.Description = description;
            body.MimeType = mimeType;

            // Set the parent folder.
            if (!String.IsNullOrEmpty(DirectoryId))
            {
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = DirectoryId } };
            }

            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(filename);
            var stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.InsertMediaUpload request = Service.Files.Insert(body, stream, mimeType);
                request.Upload();

                File file = request.ResponseBody;

                // Uncomment the following line to print the File ID.
                // Console.WriteLine("File ID: " + file.Id);

                return file;
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred: " + e.Message, e);
                return null;
            }
        }



        public File UpdateFile(string uploadFile, string fileId)
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
                                      Id = DirectoryId
                                  }
                              }
                };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
                var stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.UpdateMediaUpload request = Service.Files.Update(body, fileId, stream, GetMimeType(uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred: " + e.Message, e);
                    return null;
                }
            }
            else
            {
                Logger.Error("File does not exist: " + uploadFile);
                return null;
            }

        }

        public File UpdateFile(
            string uploadFile,
            string fileId,
            String description = "File updated by DriveUploader for Windows",
            byte[] byteArray = null)
        {

            if (System.IO.File.Exists(uploadFile))
            {
                var body = new File
                {
                    Title = System.IO.Path.GetFileName(uploadFile),
                    Description = description,
                    MimeType = GetMimeType(uploadFile),
                    Parents = new List<ParentReference>()
                              {
                                  new ParentReference()
                                  {
                                      Id = DirectoryId
                                  }
                              }
                };

                // File's content.
                // byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
                var stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.UpdateMediaUpload request = Service.Files.Update(body, fileId, stream, GetMimeType(uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred: " + e.Message, e);
                    return null;
                }
            }
            else
            {
                Logger.Error("File does not exist: " + uploadFile);
                return null;
            }

        }
        public File CreateDirectory(String directoryTitle, String directoryDescription = "Backup of files")
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
                var request = Service.Files.Insert(body);
                newDirectory = request.Execute();
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred: " + e.Message, e);
            }

            return newDirectory;
        }

        public string GetMimeType(string fileName)
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

        public File UploadFile(
            string uploadFile,
            String description = "File uploaded by DriveUploader For Windows")
        {
            if (System.IO.File.Exists(uploadFile))
            {
                var body = new File
                           {
                               Title = System.IO.Path.GetFileName(uploadFile),
                               Description = description,
                               MimeType = GetMimeType(uploadFile),
                               Parents = new List<ParentReference>()
                                         {
                                             new ParentReference()
                                             {
                                                 Id = DirectoryId
                                             }
                                         }
                           };

                byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
                var stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.InsertMediaUpload request = Service.Files.Insert(body, stream, GetMimeType(uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred: " + e.Message, e);
                    return null;
                }
            }
            else
            {
                Logger.Error("File does not exist: " + uploadFile);
                return null;
            }

        }
        public File InsertFile(
            string uploadFile,
            String description = "File uploaded by DriveUploader For Windows",
            byte[] byteArray = null)
        {

            var body = new File
            {
                Title = System.IO.Path.GetFileName(uploadFile),
                Description = description,
                MimeType = GetMimeType(uploadFile),
                Parents = new List<ParentReference>()
                                         {
                                             new ParentReference()
                                             {
                                                 Id = DirectoryId
                                             }
                                         }
            };

            //  byte[] byteArray = System.IO.File.ReadAllBytes(uploadFile);
            var stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.InsertMediaUpload request = Service.Files.Insert(body, stream, GetMimeType(uploadFile));
                request.Upload();
                return request.ResponseBody;
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred: " + e.Message, e);
                return null;
            }


        }
    }
}
