using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Dropbox.Api;
using System.Linq;

// Timer triggered Azure Function 
// TokenStoreInputBinding (tokenName scienario)  
// Using MSI to get authentication token for Token Store access, but must specify full path to token (i.e. path to service and token name) 
// Accessing dropbox files 

public static class TestTokenStoreBinding_timer
{
    [FunctionName("TestTokenStoreBinding_timer")]
    public static async void Run([TimerTrigger("*/1 * * * * * ")]TimerInfo myTimer, ILogger log, 
        [TokenStoreInputBinding(tokenUrl = "https://ameltokenstore.tokenstore.azure.net/services/dropbox/tokens/sampleToken", 
        scenario = "tokenName", identityProvider = "google")] string outputToken) 
    {
        // timer triggered every second (note: may be slowed down since this is an async method)
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (!string.IsNullOrEmpty(outputToken))
            {
                using (var dbx = new DropboxClient(outputToken))
                {
                    var list = await dbx.Files.ListFolderAsync(string.Empty);

                    // show folders then files
                    foreach (var item in list.Entries.Where(i => i.IsFolder))
                    {
                        log.LogInformation($"Directory: {item.Name}");
                    }

                    foreach (var item in list.Entries.Where(i => i.IsFile))
                    {
                        log.LogInformation($"File: {item.Name}");
                    }
                }
            }
    }
}

