using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Linq;
using System.Collections.Generic;


// Http triggered Azure Function 
// Using an IMPERATIVE TokenStore Binding
// Using MSI to get authentication token for Token Store access, using user identity to get specific token  
// Accessing onedrive files 

namespace TokenVaultFunction
{
    public class TestTokenStoreBinding_User
    {
        [FunctionName("TestTokenStoreBinding_User")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, [TokenStoreBinding(Token_url = "https://ameltokenstore.tokenstore.azure.net/services/microsoftgraph", Auth_flag = "user")] String outputToken) // had: Binder binder (for imperative binding) 
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //TokenStoreBindingAttribute attribute = new TokenStoreBindingAttribute("https://ameltokenstore.tokenstore.azure.net/services/microsoftgraph", "user", req); // Initialize TokenStore Binding
            //var outputToken = await binder.BindAsync<string>(attribute);

            var filesList = new List<string>(); // store onedrive file

            if (!string.IsNullOrEmpty(outputToken))
            {
                var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", outputToken);
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
                return (ActionResult)new OkObjectResult($"Files: {sb.ToString()}");
            }

            return outputToken != null
            ? (ActionResult)new OkObjectResult($"Token binding output is not null, {outputToken}")
            : new BadRequestObjectResult("Token binding output is NULL");
        }
    }
}
