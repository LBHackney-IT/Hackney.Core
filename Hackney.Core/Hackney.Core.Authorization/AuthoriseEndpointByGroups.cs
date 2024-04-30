using System;
using System.Linq;
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hackney.Core.Authorization
{
    public class AuthorizeEndpointByGroups : TypeFilterAttribute
    {
        /// <summary>
        /// Authorise this endpoint using permitted groups
        /// </summary>
        /// <param name="permittedGroupsVariable">
        /// The name of the environment variable that stores the permitted groups for the endpoint
        /// </param>
        public AuthorizeEndpointByGroups(string permittedGroupsVariable) : base(typeof(TokenGroupsFilter))
        {
            Arguments = new object[] { permittedGroupsVariable };
        }
    }

    public class TokenGroupsFilter : IAuthorizationFilter
    {
        private readonly string[] _requiredGoogleGroups;
        private readonly ITokenFactory _tokenFactory;

        public TokenGroupsFilter(ITokenFactory tokenFactory, string permittedGroupsVariable)
        {
            _tokenFactory = tokenFactory;

            var requiredGooglepermittedGroupsVariable = Environment.GetEnvironmentVariable(permittedGroupsVariable);
            if (requiredGooglepermittedGroupsVariable is null) throw new EnvironmentVariableNullException(permittedGroupsVariable);

            _requiredGoogleGroups = requiredGooglepermittedGroupsVariable.Split(','); // Note: Env variable must not have spaces after commas
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = _tokenFactory.Create(context.HttpContext.Request.Headers);
            if (token is null || !token.Groups.Any(g => _requiredGoogleGroups.Contains(g)))
            {
                context.Result = new UnauthorizedObjectResult($"User {token?.Name} is not authorized to access this endpoint.");
            }
        }
    }
}
