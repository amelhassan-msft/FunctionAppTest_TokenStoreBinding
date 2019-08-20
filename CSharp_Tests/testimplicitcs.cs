using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Dropbox.Api;
using System.Linq;

namespace Test
{
    public static class testimplicitcs
    {
        [FunctionName("testimplicitcs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, Binder binder)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // var Token_url = Environment.GetEnvironmentVariable("Token_url_name");
            var Token_url = "https://joetest.tokenstore.azure.net/services/dropbox/tokens/sampleToken";

            // An implicit binding is used here to access the Token_service param from app settings 
            TokenStoreInputBindingAttribute attribute = new TokenStoreInputBindingAttribute(Token_url, "tokenName", "aad"); // Initialize TokenStoreBinding

            var outputToken = await binder.BindAsync<string>(attribute);

            log.LogInformation($"The output token is: {outputToken}");

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
