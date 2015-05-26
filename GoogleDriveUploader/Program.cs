using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;

namespace GoogleDriveUploader
{
    public class Program
    {
        // http://localhost:2721/authorize/?code=4/ferZqtUQdEPajosU22Ty4xhZokQEMv37m7wM5JLNNFs.Ar-79qq_U9UdJvIeHux6iLZp7kEZmwI#
        static void Main(string[] args)
        {
            string clientId = "660481316212-ivbld0hjqll1k1u67l1l9g67cvd88gtc.apps.googleusercontent.com";
            string clientSecret = "30job5lDA-fzZNP2M7b0EQuA";
            string folderName = "MyStore";
            string applicationName = "MyStoreApplicationName";
            string folder = "MyStoreFolder";
            //UploadFile(clientId, clientSecret, folderName, applicationName, folder);

        }

        private  String UploadFile(string clientId, string clientSecret, string folderName, string applicationName, string folder)
        {

           
            return "";
        }
    }
}
