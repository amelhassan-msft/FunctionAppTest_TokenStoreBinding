using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Dropbox.Api;
using System.Linq;
using System.Collections.Generic;
using Facebook;
using System.Dynamic;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Text;

namespace Test
{
    public static class twoservices
    {
        [FunctionName("twoservices")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, [TokenStoreInputBinding(tokenUrl = "https://joetest.tokenstore.azure.net/services/dropbox",
            scenario = "user", identityProvider = "google")] string dropboxToken, [TokenStoreInputBinding(tokenUrl = "https://joetest.tokenstore.azure.net/services/microsoftgraph",
            scenario = "user", identityProvider = "google")] string microsoftToken)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var filesList = new List<string>(); // store onedrive file

            if (!string.IsNullOrEmpty(microsoftToken))
            {
                var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", microsoftToken);
                    return Task.CompletedTask;
                }));

                var driveItems = await graphClient.Me.Drive.Root.Children.Request().GetAsync();
                var driveItemNames = driveItems.Select(driveItem => driveItem.Name);

                foreach (var driveitem in driveItemNames) // add drive item names to filesList 
                {
                    filesList.Add(driveitem);
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var file in filesList)
                {
                    sb.Append(file);
                }

                if (!string.IsNullOrEmpty(dropboxToken))
                {
                    using (var dbx = new DropboxClient(dropboxToken))
                    {
                        var list = await dbx.Files.ListFolderAsync(string.Empty);

                        byte[] byteArray = Encoding.ASCII.GetBytes($"Files: {sb.ToString()}");
                        MemoryStream stream = new MemoryStream(byteArray);

                        var commit = new Dropbox.Api.Files.CommitInfo("/listonedrivefiles");
                        await dbx.Files.UploadAsync(commit, stream);
                    }
                }
            }
         
            return (ActionResult)new OkObjectResult($"Check your Dropbox account for a file \"listonedrivefiles\" to see a list of your OneDrive files");
        }
    }
}
