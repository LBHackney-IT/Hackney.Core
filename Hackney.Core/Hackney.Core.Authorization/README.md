# Hackney.Core.Authorization NuGet Package
This library allows developers to customize access to an API endpoint.

The `Hackney.Core.Authorization` NuGet package has multiple ways to limit access to an API/API endpoint. See below:

## AuthorizeByGroups

This allows a developer to limit an endpoint so that only users in certain google groups can access it. 

### Instructions

1. Make sure your API has registered the `ITokenFactory` service from `Hackney.Core.JWT`. This is usually done in the `Startup.cs` file by adding the following line of code:
	```csharp
	services.AddTokenFactory();
	```

2. Set up an environment variable to contain the list of groups that have access to an endpoint. You can name this variable anything you want. The list of groups should be comma-separated, with no whitespaces (unless the group 	name itself has whitespaces). 
	
	For example:
	```shell
	$PATCH_ENDPOINT_GOOGLE_GROUPS="group1,group2,group3,group with whitespace,group4"
	```

	You can have multiple environment variables if you want different groups to have access to different endpoints.
	_Note: Make sure you configure this environment variable to be available locally and in your pipeline, through your `serverless.yml` and `appsettings.json` files._

3. Add the authorization filter to your endpoint(s). In your controller methods, add the following line of code:
	
	```csharp
	[AuthorizeEndpointByGroups("<Environment Variable Name>")]
	```

	For example:
	```csharp
	[HttpPost]
	[AuthorizeByGroups("ALLOWED_GOOGLE_GROUPS_POST")]
	public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
	{
		 // Your code here
	}
	```

	This means that the `CreateUser` endpoint will be limited to the Google groups listed in the `$ALLOWED_GOOGLE_GROUPS_POST` environment variable.

### Testing
To test the authorization filters, you can follow these steps:

1. Create a mock JWT token for a fake user with some fake Google groups in it.
2. Set your environment variable to include the same Google groups as the ones in the JWT token.
3. Change either the environment variable or the JWT token to have a Google group that doesn't match the other.

This will allow you to test the authorization filters by simulating different scenarios and verifying that the access to the endpoints is limited correctly.

## AuthorizeEndpointByIpWhitelist

### Instructions

1. Set up an environment variable to contain the list of IP addresses that have access to an endpoint. You can name this variable anything you want.
	**Format:**
	- The list of IP addresses should be separated by semi-colons and have no whitespaces.
	- The IP addresses should be in the dotted-quad notation (`xxx.xxx.xxx.xxx`) for IPv4 or the colon-hexadecimal notation (`xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx`) for IPv6.
	
	For example:
	```shell
	$GET_ENDPOINT_WHITELISTED_IPS="127.0.0.1;243.156.218.37"
	```

	You can have multiple environment variables if you want different groups to have access to different endpoints.
	_Note: Make sure you configure this environment variable to be available locally and in your pipeline, through your `serverless.yml` and `appsettings.json` files._

3. Add the authorization filter to your endpoint(s). In your controller methods, add the following line of code:
	
	```csharp
	[AuthorizeEndpointByIpWhitelist("<Environment Variable Name>")]
	```

	For example:
	```csharp
	[HttpGet]
	["/users/{id}"]
	[AuthorizeEndpointByIpWhitelist("ALLOWED_IPS_GET")]
	public async Task<IActionResult> GetUser([FromRoute] Guid id)
	{
		 // Your code here
	}
	```

	This means that the `GetUser` endpoint will be limited to the IP addresses listed in the `$ALLOWED_IPS_GET` environment variable.

### Testing
Mocking your IP address can be difficult, so in your e2e tests you can add the following header:
```json
{
	"X-Forwarded-For": "<Fake IP Address>"
}
```
The code will check this header before checking the source IP Address, allowing you to easily mock your IP address without having to spoof it.

## UseGoogleGroupAuthorization

This allows a developer to limit an *entire* API to the google groups they specify

Note: The preferred method to manage access to an API is to use the *lambda authorizer*. You can find more information [here](https://playbook.hackney.gov.uk/API-Playbook/lambda_authoriser).

### Instructions

To use the `UseGoogleGroupAuthorization` middleware, add the following line of code to your `Configure(...)` method in the `Startup.cs` file:

```csharp
	app.UseGoogleGroupAuthorization();
```

This middleware will use `REQUIRED_GOOGL_GROUPS` environment valiable to get required Google groups list.

### Testing
To test the authorization filters, you can follow these steps:

1. Create a mock JWT token for a fake user with some fake Google groups in it.
2. Set your environment variable to include the same Google groups as the ones in the JWT token.
3. Change either the environment variable or the JWT token to have a Google group that doesn't match the other.

This will allow you to test the authorization filters by simulating different scenarios and verifying that the access to the endpoints is limited correctly.