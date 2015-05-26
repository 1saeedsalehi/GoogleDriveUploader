using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GoogleDriveUploader
{
    public class Program
    {
        // http://localhost:2721/authorize/?code=4/ferZqtUQdEPajosU22Ty4xhZokQEMv37m7wM5JLNNFs.Ar-79qq_U9UdJvIeHux6iLZp7kEZmwI#
        static void Main(string[] args)
        {
            var clientId = "660481316212-ivbld0hjqll1k1u67l1l9g67cvd88gtc.apps.googleusercontent.com";
            var clientSecret = "30job5lDA-fzZNP2M7b0EQuA";
            string folderName = "MyStore";
            string applicationName = "MyStoreApplicationName";
            string folder = "MyStoreFolder";

            var scopes = new[] { DriveService.Scope.Drive,
                                 DriveService.Scope.DriveFile};
  
            var dataStore = new FileDataStore(folder);

            var secrets = new ClientSecrets { ClientId=clientId,ClientSecret = clientSecret };

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

            var query = string.Format("title = '{0}' and mimeType = 'application/vnd.google-apps.folder'",folderName);

            var files = FileHelper.GetFiles(service, query);

            // If there isn't a directory with this name lets create one.
            if (files.Count == 0)
            {
                files.Add(UploadHelper.CreateDirectory(service, folderName));
            }

            if (files.Count != 0)
            {
                string directoryId = files[0].Id;

                File newFile = UploadHelper.UploadFile(service, @"c:\temp\Lighthouse.jpg", directoryId);

                File updatedFile = UploadHelper.UpdateFile(service, @"c:\temp\Lighthouse.jpg", directoryId, newFile.Id);
                
            }

        }
    }
}
