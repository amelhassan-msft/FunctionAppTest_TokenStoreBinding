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
    public static class testarm
    {
        [FunctionName("testarm")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, Binder binder) 
        {
            var Token_url = Environment.GetEnvironmentVariable("Token_url_name");
            // throw new InvalidOperationException($"The token url is: {Token_url}"); // as a check
            
            // An implicit binding is used here to access the Token_url param from app settings 
            TokenStoreInputBindingAttribute attribute = new TokenStoreInputBindingAttribute(Token_url, "tokenName", "google"); // Initialize TokenStore Binding

            var outputToken = await binder.BindAsync<string>(attribute);

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
