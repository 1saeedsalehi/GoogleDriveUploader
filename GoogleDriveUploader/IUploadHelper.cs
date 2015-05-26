using System;
namespace GoogleDriveUploader
{
    public interface IUploadHelper
    {
        Google.Apis.Drive.v2.Data.File CreateDirectory(string directoryTitle, string directoryDescription = "Backup of files");
        void deleteFile(string fileId);
        string GetMimeType(string fileName);
        Google.Apis.Drive.v2.Data.File insertFile(string title, string description, string mimeType, string filename);
        Google.Apis.Drive.v2.Data.File InsertFile(string uploadFile, string description = "File uploaded by DriveUploader For Windows", byte[] byteArray = null);
        System.Collections.Generic.List<Google.Apis.Drive.v2.Data.File> retrieveAllFiles();
        Google.Apis.Drive.v2.Data.File TrashFile(string fileId);
        Google.Apis.Drive.v2.Data.File updateFile(string fileId, string newTitle, string newDescription, string newMimeType, string newFilename, bool newRevision);
        Google.Apis.Drive.v2.Data.File UpdateFile(string uploadFile, string fileId);
        Google.Apis.Drive.v2.Data.File UpdateFile(string uploadFile, string fileId, string description = "File updated by DriveUploader for Windows", byte[] byteArray = null);
        Google.Apis.Drive.v2.Data.File UploadFile(string uploadFile, string description = "File uploaded by DriveUploader For Windows");
    }
}
