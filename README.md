# Hackney.Core NuGet Packages
At Hackney, we have created NuGet Packages to prevent the duplication of common code when implementing our APIs.
Hence this NuGet package will store common code that can then be used in relevant projects. 

## Contributing

### Automated Versioning
The pipeline automatically updates the version number for all packages in Hackney.Core.

Any specific version number follows the form `Major.Minor.Patch[-Suffix]`, where the components have the following meanings:

* *Major*: Breaking changes
* *Minor*: New features, but backward compatible
* *Patch*: Backwards compatible bug fixes only
* *Suffix (optional)*: a hyphen followed by a string denoting a pre-release version

### Branching Strategy

In order for the pipeline to be able to run automated tests and create preview versions of packages, you must name your branch correctly.

**Name your branch following the convention of `feature/<some-feature>`.** This will allow the pipeline to work correctly. 
If all tests pass, a new version of your package will be publised on every commit. You can see published versions of packages [here](https://github.com/orgs/LBHackney-IT/packages?repo_name=lbh-core).

All preview versions of packages will have the suffix **`-feat-<branch-name>-<number>`**.

This branch name in the package version has a character limit of **12 characters**, so try to name your branch accordingly, otherwise it will be cut off.

## Building the package
After cloning the repo, you may find many errors relating to the `Hackney.Core.Testing.PactBroker` project similar to the one below when attempting to build the solution **_on a Windows machine_**:
```cmd
C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(4965,5): warning MSB3026: Could not copy "C:\Users\<USER-NAME>\.nuget\packages\pactnet.windows\3.0.2\tools\pact-win32\lib\ruby\lib\ruby\gems\2.2.0\gems\bundler-1.9.9\lib\bundler\vendor\thor\lib\thor\actions\empty_directory.rb" to "bin\Debug\netcoreapp3.1\pact-win32\lib\ruby\lib\ruby\gems\2.2.0\gems\bundler-1.9.9\lib\bundler\vendor\thor\lib\thor\actions\empty_directory.rb". Beginning retry 1 in 1000ms. Could not find a part of the path 'bin\Debug\netcoreapp3.1\pact-win32\lib\ruby\lib\ruby\gems\2.2.0\gems\bundler-1.9.9\lib\bundler\vendor\thor\lib\thor\actions\empty_directory.rb'.
```
This is because packages required by the PactBroker client libraries contain deep folder trees and when copied to the build output folder may 
then have filenames longer than that traditionally supported by Windows.

This can be fixed by enabling long paths in the Windows registry using [these instructions](https://docs.microsoft.com/en-gb/windows/win32/fileio/maximum-file-path-limitation?tabs=cmd#enable-long-paths-in-windows-10-version-1607-and-later). 
Once enabled it will take effect *without* needing to restart Visual Studio.

## Using the packages
For full details on how to use the package(s) within this repository please read 
[this wiki page](https://github.com/LBHackney-IT/lbh-core/wiki/Using-the-package(s)-from-the-Hackney.Core-repository).

*Note: The Hackney.Core project has been split into individual packages and is now deprecated.*
*In order to use our packages, import each Hackney.Core dependency required individually.*

## Adding a new package
Please refer to [our documentation](https://docs.google.com/document/d/1aJzhNxmSq_D4porwSIa528xYwTghblJmky4decvQBlA/edit?usp=sharing) on creating NuGet packages. For this repository, create your project folder in the `Hackney.Core` folder and test folder in `Hackney.Core.Tests`. Use the [workflow template](.github/workflows-template/publish.yml) to create your own workflow file in the `.github/workflows` folder.

## Features

The following features are implemented within this package (Alphabetical Order):
* [Authorisation](/Hackney.Core/Hackney.Core.Authorization/README.md)
* [DynamoDb](#DynamoDb)
    * [Converters](#Converters)
    * [Health check](#DynamoDb-Health-check)
    * [Paged results](#Paged-results)
* [ElasticSearch](/Hackney.Core/Hackney.Core.ElasticSearch/README.md)
* [Enums](/Hackney.Core/Hackney.Core.Enums/README.md)
* [Health check helpers](#Health-check-helpers)
* [Http](/Hackney.Core/Hackney.Core.Http/README.md)
* [JWT](#JWT)
    * [Token Factory](#Token-Factory)
* [Logging](#Logging)
    * [Lambda logging](#Lambda-logging)
* [MVC Middleware](#MVC-Middleware)
    * [Correlation middleware](#correlation-middleware])
    * [Exception middleware](#Exception-middleware)
* [Sns](/Hackney.Core/Hackney.Core.Sns/README.md)
* Testing
    * [Hackney.Core.Testing.DynamoDb](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.DynamoDb/README.md)
    * [Hackney.Core.Testing.PactBroker](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.PactBroker/README.md)
    * [Hackney.Core.Testing.Shared](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Shared/README.md)
    * [Hackney.Core.Testing.Sns](/Hackney.Core/Hackney.Core.Testing/Hackney.Core.Testing.Sns/README.md)
* [Validation](/Hackney.Core/Hackney.Core.Validation/README.md)
* [Validation.AspNet](/Hackney.Core/Hackney.Core.Validation.AspNet/ReadMe.md)

### MVC Middleware

**Project reference: `Hackney.Core.Middleware`**

There are a number of different middleware classes implemented here. 

Please bear in mind that the order in which they are used within an application's `Startup.Configure()` method dictates the order in which they are executed when the application receives an HTTP request.

#### Correlation middleware

The correlation middleware can be used to ensure that all incoming HTTP requests contain a correlation id value.
If a request does not contain a guid HTTP header value with the key `x-correlation-id` then one is generated and added to the request headers.
The correlation id is also automatically added to the response headers.

*Correlation Id* - This is a (guid) value used to uniquely identify each request.

##### Usage

```csharp
using Hackney.Core.Middleware.Correlation;

namespace SomeApi
{
    public class Startup
    {
        ...
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseCorrelationId();
            ...
        }
    }
}

```

#### Exception middleware

The exception middleware can be used to set up a custom exception handler that will be used in the event of any unhandled exception.
It will log the exception and then return a standard error response that looks like the following.
```json
{
    "message": "String '2020-09-0 17:33:44' was not recognized as a valid DateTime.",
    "traceId": "0HM8BQ8L7EAEO:00000001",
    "correlationId": "3fbf8755-eb41-4c03-be9f-d0ccae470e39",
    "statusCode": 500
}
```

| Property       | Description |
| :---           | :---        |
| message        | The message property from the unhandled exception. |
| traceId        | The traceId from the original HttpRequest. |
| correlationId  | The correlationId for the request. |
| statusCode     | The HttpResponse status code. |

##### Usage

If required, the [correlation middleware](#correlation-middleware]) call should go before the exception handler to ensure that any error logged will also include the correlation id

```csharp
using Hackney.Core.Middleware.Exception;

namespace SomeApi
{
    public class Startup
    {
        ...
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseCustomExceptionHandler();
            ...
        }
    }
}

```

#### Logging scope middleware
The logging scope middleware sets up a logging scope for every incoming HTTP request. 
This means that every log statement made within that scope (i.e. during the HTTP request processing) 
will include an addition string that contains both the correlation id (and user email, if available 
in the headers) of the caller.
This means that all other logging need not concern itself without having to add this data as it is already included.

###### Usage
When used in conjunction with the [correlation middleware](#correlation-middleware]), the call to 
`UseLoggingScope()` should come _after_ the call to `UseCorrelationId()`.

```csharp
using Hackney.Core.Middleware.Logging;

namespace SomeApi
{
    public class Startup
    {
        ...
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseLoggingScope();
            ...
        }
    }
}

```

### DynamoDb

**Project reference: `Hackney.Core.DynamoDb`**

#### Setting up DynamoDb support

There is a `ConfigureDynamoDB()` extension method provided to facilitate setting up an application to use AWS DynamoDb.
By calling it in the application startup, the following interfaces will be configured in the DI container:
* `IAmazonDynamoDB`
* `IDynamoDBContext`

By default it assumes there is an appropriate AWS profile configured where the application will run. 
See [here](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html) for more details.
This means that, at the very least, your application must have a region specified in its appsettings.json:
```json
  "AWS": {
    "Region": "eu-west-2"
  }
```

###### Usage
```csharp
using Hackney.Core.DynamoDb;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.ConfigureDynamoDB();
            ...
        }
    }
}

```

##### Local mode
If there is a local DynamoDb instance available then this can be used by amending the application settings as shown below:
```json
  "AWS": {
    "Region": "DefaultRegion"
  },
  "DynamoDb": {
    "LocalMode": true,
    "LocalServiceUrl": "http://localhost:8000"
  }
```
The `LocalMode` flag must be set to `true` and the `LocalServiceUrl` should point to the local DynamoDb instance. 
These 2 values can also be set as environment variables.

#### Converters

There are a number of different converters provided to make using the `IDynamoDBContext` easier.
They are used against a property on a model class to tell the underlying AWS libraries how to marshal the property into and out of the database.

##### DynamoDbBoolConverter

This should be used on `bool` properties.
The default AWS `bool` processing converts the bool value to an integer of 0 or 1, which may or may not be a problem in itself depending on your specific data requirements.
However if you need to filter your database query on a bool property, this attribute will need to be decorating it. 
This will mean that the bool value will get serialsed into and out of the database properly (as "true" or "false") and it will also mean that the property can be used as needed in any filter.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbBoolConverter))]
    public bool IsActive { get; set; }
```

##### DynamoDbDateTimeConverter

This should be used on `DateTime` properties.
The default AWS `DateTime` processing is very restrictive, expects that value to be in a very specific format and will throw an exception if it is not in that format.
This converter will always store in the format `yyyy-MM-ddTHH:mm:ss.fffffffZ`, but is much more forgiving when reading data out of the database.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
    public DateTime Created { get; set; }
```

##### DynamoDbEnumConverter

This should be used on `enum` properties.
It ensures that an enum value is stored in the database as the string representation of the value rather than the numeric value.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbEnumConverter<TargetType>))]
    public TargetType TargetType { get; set; }
```

##### DynamoDbEnumListConverter

This should be used on properties that are a `List<>` of `enum` values.
Internally it operates in the same way as the `DynamoDbEnumConverter`.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbEnumListConverter<PersonType>))]
    public List<PersonType> PersonTypes { get; set; } = new List<PersonType>();
```

##### DynamoDbObjectConverter

This should be used on properties that are a custom object.
The coverter works by simply serialising the object to and from the database using Json.
It does this because the native AWS functionality for nested objects is very simplistic and does not honour the `LowerCamelCaseProperties` value set on the root class.
By simply converting the entire sub-object using Json we bypass these limitations.
However this will mean that any DynamoDb converters set against properties on nested objects will not be used.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbObjectConverter<AuthorDetails>))]
    public AuthorDetails Author { get; set; }
```

##### DynamoDbObjectListConverter

This should be used on properties that are a lists of custom objects.
Internally it operates in the same way as the `DynamoDbObjectConverter`.

###### Usage
```csharp
    [DynamoDBProperty(Converter = typeof(DynamoDbEnumListConverter<PersonType>))]
    public List<PersonType> PersonTypes { get; set; } = new List<PersonType>();
```

#### Paged results
The implementation of querying for paged results within DynamoDb is limited compared to other database technologies.
In essence if you query a table with a specific page size, if there are potentially more results beyond that page size limit
then a token will be returned along with the results. This token (it is a json object) indicates where in the full results list 
the current set of results ended. If this token is supplied in the next query then the list of results will start from where the 
last set finished.

##### PagedResult
This is a template class used to encapsulate the results of a paged query.

|Property|Description|
|:---|:---|
|Results|The list of result objects from the query.|
|PaginationDetails|A `PaginationDetails` object that may or may not contain a token.|

##### PaginationDetails
This class encapsulates the results of a paged query.
It contains static methods to encode or decode a token value as needed.

|Property|Description|
|:---|:---|
|HasNext|Boolen value indicating whether or not there is a next token.|
|NextToken|The token returned by the call to the AWS SDK encoded as a Base64 string.</br>`null` if there is no token.|

##### GetPagedQueryResultsAsync
This is an extension method on the `IDynamoDBContext` interface that is used to make a paged query against the database 
and returns a `PagedResult` object.

If there are no more results after the query has been made (regardless of the specified page size) then the 
`PaginationDetails.NextToken` will be null.

###### Usage
```csharp
public async Task<PagedResult<NoteDb>> GetNotesByTargetIdAsync(GetNotesByTargetIdQuery query)

{
    _logger.LogDebug($"Querying DynamoDB for notes for TargetId {query.TargetId}");

    int pageSize = query.PageSize.HasValue ? query.PageSize.Value : MAX_RESULTS;
    var queryConfig = new QueryOperationConfig
    {
        IndexName = GETNOTESBYTARGETIDINDEX,
        BackwardSearch = true,
        ConsistentRead = true,
        Limit = pageSize,
        PaginationToken = PaginationDetails.DecodeToken(query.PaginationToken),
        Filter = new QueryFilter(TARGETID, QueryOperator.Equal, query.TargetId)
    };

    return await _dynamoDbContext.GetPagedQueryResultsAsync<NoteDb>(queryConfig).ConfigureAwait(false);
}
```

#### DynamoDb Health Check
There is a `DynamoDbHealthCheck` class implemented that uses the 
[Microsoft Health check framework](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.2).
The check verifies that the required DynamoDb table is accessible by performing a `DescribeTable` call.

##### Usage
The template argument supplied to the `AddDynamoDbHealthCheck()` call is the name of a database model class that has the `DynamoDbTable` 
attribute applied to it. The method uses this attribute to determine the table name to use to query the database.

```csharp
using Hackney.Core.DynamoDb.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddDynamoDbHealthCheck<NoteDb>();
            ...
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
            ...
        }
    }
}

```

### Health Check Helpers

**Project reference: `Hackney.Core.HealthCheck`**

The default HTTP response from the Microsoft Health check framework is simply a headline `HealthStatus` value with the appropriate Http status code.

In order to provide more meaningful response information a custom response writer, the `HealthCheckResponseWriter.WriteResponse` static method,  
has been implemented to serialise the entire `HealthReport` as json. 

The only differences between the framework `HealthReport` class and the serialised response are:
* Durations are given in milliseconds only
* Any exception object has been replaced with just the exception message.

#### Usage
```csharp
using Hackney.Core.DynamoDb.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SomeApi
{
    public class Startup
    {
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponse
                });
            });
            ...
        }
    }
}

```

### JWT

**Project reference: `Hackney.Core.JWT`**

#### Token Factory
The `TokenFactory` implementation of the `ITokenFactory` interface is designed to easily retrieve a JWT token sent in the headers of an Http request.
The `ITokenFactory` interface is made available by using the `AddTokenFactory()` extension method during your application start-up.

Usage

```csharp
using Hackney.Core.JWT;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddTokenFactory();
            ...
        }
    }
}

```

### Logging

**Project reference: `Hackney.Core.Logging`**

#### Lambda logging

There is a helper method that can be used durung application startup to configure the generic Microsoft `ILogger`
framework to log to the AWS Lambda logger. By making use of the standard Microsoft implementation, it will also 
make use of the any [standard logging configuration](#https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#configure-logging) 
in the appsettings.json configuration file.

The `ConfigureLambdaLogging()` extension method will set up logging so to use the Lambda logger 
(as well as logging to debug output, and the console if the application is running in the development environment).

###### Usage
```csharp
using Hackney.Core.Logging;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.ConfigureLambdaLogging(Configuration);
            ...
        }
    }
}

```

#### Log call aspect
By making use of the [Aspect Injector](#https://github.com/pamidur/aspect-injector) library it is possible 
to easily add method logging with a single line of code.
This method logging generates simple log statements for the start and end of the decorated method. 
It does this by generating (at compile time) a proxy around the class and then calling a custom aspect 
before and after a decorated method. It is this custom aspect that will perform the logging.

In this way it is possible to easily add method logging without polluting methods with code that would 
have to be continually replicated.

**Note:**
Because the aspect proxy is generated at compile time, this **will** affect how unit tests are written. 
Any unit tests that are on a class that uses the `[LogCall]` attribute (regardless of whether or not they are testing 
a decorated method) must also ensure that the DI container used by the aspect is configured appropriately.

##### Usage

###### Setup
The first call adds the necessary DI container registrations and the second call ensures that the DI
container used to inject the `ILogger` into the custom aspect is the same one that is created in 
the application startup.

```csharp

using Hackney.Core.Middleware.Logging;

namespace SomeApi
{
    public class Startup
    {
        ...
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddLogCallAspect();
            ...
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            app.UseLogCall();
            ...
        }
    }
}

```

###### Decorate a method
To add method logging to a method simply decorate the method with the `[LogCall]` attribute.
```csharp
using Hackney.Core.Logging;

namespace SomeApi
{
    public class SomeClass
    {
        // The default log level is Trace.
        [LogCall]
        public void SomeMethod()
        {
            ...
        }

        // It is possible specify the log level required on the attribute.
        [LogCall(LogLevel.Information)]
        public void SomeOtherMethod()
        {
            ...
        }
    }
}

```

