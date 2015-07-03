using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace GoogleDriveUploader
{
    public interface IUploadHelper
    {
        void deleteFile(string fileId);
        string GetMimeType(string fileName);
        GoogleDriveFile insertFile(string title, string description, string mimeType, string filename);
        GoogleDriveFile InsertFile(string uploadFile, string description = "File uploaded by DriveUploader For Windows", byte[] byteArray = null);
        List<GoogleDriveFile> retrieveAllFiles();
        GoogleDriveFile TrashFile(string fileId);
        GoogleDriveFile updateFile(string fileId, string newTitle, string newDescription, string newMimeType, string newFilename, bool newRevision);
        GoogleDriveFile UpdateFile(string uploadFile, string fileId);
        GoogleDriveFile UpdateFile(string uploadFile, string fileId, string description = "File updated by DriveUploader for Windows", byte[] byteArray = null);
        GoogleDriveFile UploadFile(string uploadFile, string description = "File uploaded by DriveUploader For Windows");
        void Connect(
            String clientId,
            String userEmail,
            String serviceAccountEmail,
            X509Certificate2 certificate,
            String folderName,
            String password);

    }
}
