# FunctionAppTest_TokenStoreBinding
## A publishable Azure Function App with various functions and scenarios to test the Token Store input binding. The TokenStoreBinding can currently be used with Azure Funtions written in C# and JavaScript. 

To use the TokenStoreBinding extension with your own Azure Functions, follow these steps: 

Your Azure Functions must be created locally. 

### ~ C# Azure Functions ~ 
Open your project in Visual Studios. OPen the Solution Explorer tab and right click on your project name. Click "Manage NuGet Packages" and browse for "Amel.TokenStoreBinding.Net".
Install the latest version. 

### ~JavaScript Azure Functions~
#### Using a command line tool, navigate to your project folder. Run the following commands to generate the extensions.csproj file. 
- func extensions install -p Amel.TokenStoreBinding.Net -v 1.0.1
- func extensions sync
#### Other useful commands: 
- donet restore 
	- Dotnet restore
		- Restores dependencies and tools of a project
	- Dotnet build 
		- Builds a .net core application 

#### To use the binding in one of your Azure Functions, fill out the binding parameters in the corresponding host.json:  
	{
		"type": "TokenStoreBinding",
		"direction": "in",
		"name": "TokenBindingOutput",
		"Token_url": "https://ameltokenstore.tokenstore.azure.net/services/dropbox/tokens/sampletoken",
		"Auth_flag": "MSI"
	}