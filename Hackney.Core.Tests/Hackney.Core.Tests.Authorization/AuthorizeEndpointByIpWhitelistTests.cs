using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Hackney.Core.Testing.Shared;
using Microsoft.AspNetCore.Http;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using RouteData = Microsoft.AspNetCore.Routing.RouteData;
using Hackney.Core.Authorization.Exceptions;

namespace Hackney.Core.Authorization.Tests
{
    public class AuthorizeEndpointByIpWhitelistTests
    {
        IpWhitelistFilter _classUnderTest;
        Mock<ILogger<IpWhitelistFilter>> _logger;

        private AuthorizationFilterContext _context;
        private HeaderDictionary _requestHeaders = new HeaderDictionary();
        private Mock<HttpContext> _mockHttpContext = new Mock<HttpContext>();

        private readonly string _whitelistEnvVar = "WHITELIST";
        private readonly string _whitelistedIp = "192.168.1.1";

        public AuthorizeEndpointByIpWhitelistTests()
        {
            Environment.SetEnvironmentVariable(_whitelistEnvVar, _whitelistedIp);
            _logger = new Mock<ILogger<IpWhitelistFilter>>();
            _classUnderTest = new IpWhitelistFilter(_logger.Object, _whitelistEnvVar);

            SetUpMockContextAndHeaders();
        }

        private void SetUpMockContextAndHeaders()
        {
            _mockHttpContext.Setup(x => x.Request.Headers).Returns(_requestHeaders);
            var actionContext = new ActionContext(_mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            _context = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void ShouldThrowExceptionWhenWhitelistVariableIsMissing()
        {
            var missingVar = "MISSING_VARIABLE";
            Func<IpWhitelistFilter> func = () => new IpWhitelistFilter(_logger.Object, missingVar);
            func.Should().Throw<EnvironmentVariableNullException>().WithMessage($"Cannot resolve {missingVar} environment variable.");
        }

        [Fact]
        public void OnAuthorization_ReturnsUnauthorizedResult_WhenRemoteIpIsNull()
        {
            // Arrange
            _mockHttpContext.Setup(x => x.Connection.RemoteIpAddress)
                .Returns((IPAddress)null);

            // Act
            _classUnderTest.OnAuthorization(_context);

            // Assert
            var result = _context.Result;
            _context.Result.Should().BeOfType(typeof(UnauthorizedObjectResult));
            (_context.Result as UnauthorizedObjectResult)?.Value.Should().Be("Remote IP address is null.");
            _logger.VerifyExact(LogLevel.Error, "Failed to retrieve Remote IP address.", Times.Once());
        }


        [Fact]
        public void ShouldAllowRequestFromWhitelistedIp()
        {
            _mockHttpContext.Setup(x => x.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse(_whitelistedIp));
            _classUnderTest.OnAuthorization(_context);

            _context.Result.Should().BeNull();
            _logger.VerifyExact(LogLevel.Information, $"Request from Remote IP address: {_whitelistedIp}", Times.Once());
        }

        [Fact]
        public void ShouldDenyRequestFromNonWhitelistedIp()
        {
            var remoteIp = "135.135.75.55";
            _mockHttpContext.Setup(x => x.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse(remoteIp));
            _classUnderTest.OnAuthorization(_context);

            _context.Result.Should().BeOfType(typeof(UnauthorizedObjectResult));
            (_context.Result as UnauthorizedObjectResult)?.Value.Should().Be($"IP Address {remoteIp} is not authorized to access this endpoint.");
            _logger.VerifyExact(LogLevel.Warning, $"Forbidden Request from Remote IP address: {remoteIp}", Times.Once());
        }
    }
}
