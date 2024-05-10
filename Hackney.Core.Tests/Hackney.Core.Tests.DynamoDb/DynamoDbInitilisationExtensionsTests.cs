using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using Hackney.Core.DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb
{
    public class DynamoDbInitilisationExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("false")]
        [InlineData("true")]
        public void ConfigureDynamoDBTestNoLocalModeEnvVarUsesAWSService(string localModeEnvVar)
        {
            Environment.SetEnvironmentVariable("DynamoDb_LocalMode", localModeEnvVar);

            ServiceCollection services = new ServiceCollection();
            services.ConfigureDynamoDB();

            services.Any(x => x.ServiceType == typeof(IAmazonDynamoDB)).Should().BeTrue();
            var sd = services.First(x => x.ServiceType == typeof(IAmazonDynamoDB));
            sd.Lifetime.Should().Be(ServiceLifetime.Singleton);
            sd.ImplementationFactory.Should().NotBeNull();

            services.Any(x => x.ServiceType == typeof(IDynamoDBContext)).Should().BeTrue();
            sd = services.First(x => x.ServiceType == typeof(IDynamoDBContext));
            sd.Lifetime.Should().Be(ServiceLifetime.Scoped);
            sd.ImplementationFactory.Should().NotBeNull();

            Environment.SetEnvironmentVariable("DynamoDb_LocalMode", null);
        }

        [Fact]
        public void ConfigureDynamoDBTestRegistersServices()
        {
            string url = "http://localhost:8000/";
            Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", url);
            Environment.SetEnvironmentVariable("DynamoDb_LocalMode", "true");

            ServiceCollection services = new ServiceCollection();
            services.ConfigureDynamoDB();
            var serviceProvider = services.BuildServiceProvider();

            var amazonDynamoDB = serviceProvider.GetService<IAmazonDynamoDB>();
            amazonDynamoDB.Should().NotBeNull();
            amazonDynamoDB.Config.ServiceURL.Should().Be(url);

            var dynamoDBContext = serviceProvider.GetService<IDynamoDBContext>();
            dynamoDBContext.Should().NotBeNull();

            Environment.SetEnvironmentVariable("DynamoDb_LocalServiceUrl", null);
            Environment.SetEnvironmentVariable("DynamoDb_LocalMode", null);
        }
    }
}
