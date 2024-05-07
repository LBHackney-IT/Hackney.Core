using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hackney.Core.Authorization.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Hackney.Core.Authorization
{
    /// <summary>
    /// Represents a class that authorizes an endpoint based on an IP whitelist.
    /// </summary>
    public class AuthorizeEndpointByIpWhitelist : TypeFilterAttribute
    {
        /// <summary>
        /// Attribute that wraps the <see cref="Hackney.Core.Authorization.AuthorizeEndpointByIpWhitelist"/> to authorize an endpoint based on an IP whitelist.
        /// </summary>
        /// <param name="ipWhitelistVariableName">The name of the IP whitelist variable.</param>
        public AuthorizeEndpointByIpWhitelist(string ipWhitelistVariableName) : base(typeof(IpWhitelistFilter))
        {
            Arguments = new object[] { ipWhitelistVariableName };
        }
    }

    /// <summary>
    /// Represents a class that filters requests based on an IP whitelist.
    /// </summary>
    public class IpWhitelistFilter : IAuthorizationFilter
    {
        public readonly List<string> Whitelist;
        private readonly ILogger<IpWhitelistFilter> _logger;

        public IpWhitelistFilter(ILogger<IpWhitelistFilter> logger, string ipWhitelistVariableName)
        {
            _logger = logger;

            var whitelist = Environment.GetEnvironmentVariable(ipWhitelistVariableName)
                ?? throw new EnvironmentVariableNullException(ipWhitelistVariableName);

            whitelist = Regex.Replace(whitelist, @"\s+", ""); // remove any whitespace
            var ips = whitelist.Split(';');
            Whitelist = new List<string>(ips);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (remoteIp == null)
            {
                _logger.LogError("Failed to retrieve Remote IP address.");
                context.Result = new UnauthorizedObjectResult("Remote IP address is null.");
                return;
            }

            _logger.LogInformation("Request from Remote IP address: {RemoteIp}", remoteIp);

            if (!Whitelist.Contains(remoteIp))
            {
                _logger.LogWarning(
                    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Result =
                    new UnauthorizedObjectResult($"IP Address {remoteIp} is not authorized to access this endpoint.");
            }
        }
    }
}
