using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Dropbox.Api;
using System.Collections.Generic;
using Facebook;
using System.Text;

namespace Test
{
    public static class twoservices
    {
        // This is an Http triggered Azure Function
        // Two TokenStoreInputBindings are being used to get an access token for Dropbox and Facebook 
        // The user scenario TokenStoreInputBinding is being used, users are prompted to login with their Google account 
        // Does: User profile info is read using the Facebook api and this information is stored in a file that is uploaded to dropbox 

        [FunctionName("twoservices")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, [TokenStoreInputBinding(tokenUrl = "https://joetest.tokenstore.azure.net/services/dropbox",
            scenario = "user", identityProvider = "google")] string dropboxToken, [TokenStoreInputBinding(tokenUrl = "https://joetest.tokenstore.azure.net/services/facebook",
            scenario = "user", identityProvider = "google")] string facebookToken)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var filesList = new List<string>(); // store onedrive file

            if (!string.IsNullOrEmpty(facebookToken))
            {
                // Extract Facebook info and save it to a file in DropBox 
                string fbName, fbEmail, fbBirthday, fbHometown;
                try
                {
                    var fb = new FacebookClient(facebookToken);
                    var result = (IDictionary<string, object>)fb.Get("/me?fields=name,birthday,email,hometown");
                    fbName = (string)result["name"];
                    fbEmail = (string)result["email"];
                    fbBirthday = (string)result["birthday"];
                    // Hometown 
                    JsonObject townData = (JsonObject)result["hometown"];
                    var test = townData["name"].ToString();
                    fbHometown = test;
                }
                catch (FacebookOAuthException)
                {
                    return null;
                }

               // Upload facebook user info to a file in dropbox 
                if (!string.IsNullOrEmpty(dropboxToken))
                {
                    using (var dbx = new DropboxClient(dropboxToken))
                    {
                        var list = await dbx.Files.ListFolderAsync(string.Empty);

                        byte[] byteArray = Encoding.ASCII.GetBytes($"Facebook user info ... " + System.Environment.NewLine + $"Name: {fbName}, Email: {fbEmail}, Birthday: {fbBirthday}, Hometown: {fbHometown}"); // List files here 
                        MemoryStream stream = new MemoryStream(byteArray);

                        var commit = new Dropbox.Api.Files.CommitInfo("/FacebookUserInfo");
                        await dbx.Files.UploadAsync(commit, stream);
                    }
                }
            }
            return (ActionResult)new OkObjectResult($"Check your Dropbox account for a file \"FacebookUserInfo\" to see your saved Facebook profile information");
        }
    }
}
