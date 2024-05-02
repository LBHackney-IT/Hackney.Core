# Hackney.Core.Authorization NuGet Package

The `Hackney.Core.Authorization` library enables developers to customize access to API endpoints. 
It provides three main features:

## AuthorizeEndpointByGroups

This feature enables developers to restrict endpoint access to users within specific Google groups.

### Instructions

1. **Configure `ITokenFactory`:** Ensure that your API has registered the `ITokenFactory` service from `Hackney.Core.JWT`. Typically, this is done in the `Startup.cs` file by adding the following code:
	```csharp
	services.AddTokenFactory();
	```

2. **Set Environment Variable:** Define an environment variable containing the list of groups with access to an endpoint. The groups should be comma-separated and have no whitespaces (unless the group name has them)
	```shell
	$PATCH_ENDPOINT_GOOGLE_GROUPS="group1,group2,group3,group with whitespace,group4"
	```

	You can have multiple environment variables if you want different groups to have access to different endpoints.
	_Note: Make sure you configure this environment variable to be available locally and in your pipeline, through your `serverless.yml` and `appsettings.json` files._
3. **Apply Authorization Filter:** In your controller methods, add the `[AuthorizeEndpointByGroups("<Environment Variable Name>")]` attribute.
	```csharp
	[HttpPost]
	[AuthorizeEndpointByGroups("ALLOWED_GOOGLE_GROUPS_POST")]
	public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
	{
		 // Your code here
	}
	```

	This means that the `CreateUser` endpoint will be limited to the Google groups listed in the `$ALLOWED_GOOGLE_GROUPS_POST` environment variable.
### Testing

To test authorization filters:

1. Create a mock JWT token for a fake user with fake Google groups.
2. Set the environment variable to include the same Google groups as those in the JWT token.
3. Modify either the environment variable or the JWT token to include a Google group that doesn't match the other.

## AuthorizeEndpointByIpWhitelist

### Instructions

1. **Set Environment Variable:** Define an environment variable containing the list of IP addresses with access to an endpoint. The list should be semi-colon separated. For example:
	```shell
	$GET_ENDPOINT_WHITELISTED_IPS="127.0.0.1;243.156.218.37"
	```

	You can have multiple environment variables if you want different groups to have access to different endpoints.
	_Note: Make sure you configure this environment variable to be available locally and in your pipeline, through your `serverless.yml` and `appsettings.json` files._

2. **Apply Authorization Filter:** In your controller methods, add the `[AuthorizeEndpointByIpWhitelist("<Environment Variable Name>")]` attribute. For example:
	```csharp
	[HttpGet]
	[Route("/users/{id}")]
	[AuthorizeEndpointByIpWhitelist("ALLOWED_IPS_GET")]
	public async Task<IActionResult> GetUser([FromRoute] Guid id)
	{
		 // Your code here
	}
	```

	This means that the `GetUser` endpoint will be limited to the IP addresses listed in the `$ALLOWED_IPS_GET` environment variable.

### Testing

To configure a fake IP address for End-to-End (E2E) testing, follow these steps:

1. **Create a MockWebApplicationFactoryWithMiddleware Class:**
	Create a `MockWebApplicationFactoryWithMiddleware` class which extends `MockWebApplicationFactory<TStartup>` and allows you to dynamically add middleware to your startup class. Also, set up any environment variables needed.

	```csharp
	public class MockWebApplicationFactoryWithMiddleware<TStartup> : MockWebApplicationFactory<TStartup> where TStartup : class
	{
		 public MockWebApplicationFactoryWithMiddleware() : base()
		 {
			  EnsureEnvVarConfigured("ALLOWED_IPS_GET", "127.0.0.1");
		 }

		 protected override void ConfigureWebHost(IWebHostBuilder builder)
		 {
			  base.ConfigureWebHost(builder);

			  builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
					.UseStartup<MiddlewareConfigurationStartup>();

			  builder.ConfigureServices(services =>
			  {
					services.AddSingleton<Action<IApplicationBuilder>>(app =>
					{
						 // Add middleware to the pipeline (can have multiple)
						 app.UseMiddleware<OverrideIpAddressMiddleware>();
					});
			  });
		 }
	}
	```

2. **Middleware Configuration:**
	To allow you to configure middleware in your startup code, create a `MiddlewareConfigurationStartup` class which inherits from `Startup`. 
	_(You can copy and paste this code directly without any changes)_

	```csharp
	public class MiddlewareConfigurationStartup : Startup
	{
		 public MiddlewareConfigurationStartup(IConfiguration configuration) : base(configuration)
		 {
		 }

		 public override void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
		 {
			  var middlewareConfigurationAction = app.ApplicationServices.GetService<Action<IApplicationBuilder>>();
			  middlewareConfigurationAction?.Invoke(app);

			  base.Configure(app, env, logger);
		 }
	}
	```

3. **Create Override IP Address Middleware:**
	The `OverrideIpAddressMiddleware` class is responsible for setting the remote IP address to a fake value. 
	_(You can copy and paste this code directly without any changes)_
	Make sure to reference it in the `ConfigureWebHost` method.

	```csharp
	public class OverrideIpAddressMiddleware
	{
		 private readonly RequestDelegate _next;

		 public OverrideIpAddressMiddleware(RequestDelegate next)
		 {
			  _next = next;
		 }

		 public async Task Invoke(HttpContext context)
		 {
			  byte[] ipBytes = { 127, 0, 0, 1 };
			  IPAddress ipAddress = new IPAddress(ipBytes);
			  context.Connection.RemoteIpAddress = ipAddress;
			  await _next(context);
		 }
	}
	```

4. **Create an AppTestCollection:**
	This step involves defining a collection of tests that share a common context. In this case, the context is a `MockWebApplicationFactoryWithMiddleware<Startup>.` This factory sets up a mock web application with middleware for testing.

	```csharp
	[CollectionDefinition("AppTest middleware collection", DisableParallelization = true)]
	public class AppTestCollectionMiddleware : ICollectionFixture<MockWebApplicationFactoryWithMiddleware<Startup>>
	{
		 // This class has no code, and is never created. Its purpose is simply
		 // to be the place to apply [CollectionDefinition] and all the
		 // ICollectionFixture<> interfaces.
	}
	```

5. **Update your references in your E2E test.**
	In your end-to-end (E2E) tests, you need to update the collection reference to use the new `AppTestCollectionMiddleware` you just created. This ensures that the tests use the mocked IP address provided by the middleware.

	For example, if your test class currently looks like this:

	```csharp
	[Collection("AppTest collection")]
	// ...
	public FetchAllContactDetailsTests(MockWebApplicationFactory<Startup> appFactory)
	```
	
	You should update the [Collection] attribute to reference the "AppTest middleware collection", and the `appFactory` parameter to have a type of `MockWebApplicationFactoryWithMiddleware<Startup>`:

	```csharp
	[Collection("AppTest middleware collection")]
	// ...
	public FetchAllContactDetailsTests(MockWebApplicationFactoryWithMiddleware<Startup> appFactory)
	```

	With this change, the tests in this class will now use the mocked IP address.

## UseGoogleGroupAuthorization

This feature enables developers to restrict access to an entire API based on specified Google groups.

**Note:** While this method is available, the preferred approach is using the lambda authorizer. Refer to the [API Playbook](https://playbook.hackney.gov.uk/API-Playbook/lambda_authoriser) for details.

### Instructions

To use the `UseGoogleGroupAuthorization` middleware, add the following line to your `Configure(...)` method in `Startup.cs`:

```csharp
	app.UseGoogleGroupAuthorization();
```

This middleware will use `REQUIRED_GOOGL_GROUPS` environment variable to get required Google groups list.

### Testing
The testing process for `AuthorizeEndpointByIpWhitelist` is similar to that of `AuthorizeEndpointByGroups`. For detailed instructions on how to conduct these tests, please refer to the [Testing section](#testing).