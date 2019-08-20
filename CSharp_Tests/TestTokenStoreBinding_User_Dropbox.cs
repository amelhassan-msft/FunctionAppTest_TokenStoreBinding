using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Dropbox.Api;
using System.Collections.Generic;
using System.Linq;
// Http triggered Azure Function
// TokenStoreInputBinding (user scienario)  
// Accessing dropbox files 

namespace Test
{
    public static class TestTokenStoreBinding_User_Dropbox
    {
        [FunctionName("TestTokenStoreBinding_User_Dropbox")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, [TokenStoreInputBinding(tokenUrl = "https://joetest.tokenstore.azure.net/services/dropbox",
            scenario = "user", identityProvider = "google")] string outputToken)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var filesList = new List<string>();

            if (!string.IsNullOrEmpty(outputToken))
            {
                using (var dbx = new DropboxClient(outputToken))
                {
                    var list = await dbx.Files.ListFolderAsync(string.Empty);

                    // show folders then files
                    foreach (var item in list.Entries.Where(i => i.IsFolder))
                    {
                        filesList.Add($"{item.Name}/");
                    }

                    foreach (var item in list.Entries.Where(i => i.IsFile))
                    {
                        filesList.Add($"{item.Name} \n");
                    }
                }
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var file in filesList)
            {
                sb.Append(file);
            }
            return (ActionResult)new OkObjectResult($"Files: \n {sb.ToString()}");
        }
    }
}
