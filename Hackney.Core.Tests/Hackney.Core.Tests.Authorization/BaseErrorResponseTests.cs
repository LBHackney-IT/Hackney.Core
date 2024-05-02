using FluentAssertions;
using Hackney.Core.Authorization;
using System.Net;
using Hackney.Core.Authorization.Exceptions;
using Xunit;

namespace Hackney.Core.Tests.Authorization
{
    public class BaseErrorResponseTests
    {
        [Theory]
        [InlineData("Message", (int)HttpStatusCode.Forbidden, "Details", "Details")]
        [InlineData("Message", (int)HttpStatusCode.Forbidden, null, "")]
        public void BaseErrorResponseConstructorWithDifferentDetails(string expectedMessage, int expectedStatusCode,
            string actualDetails, string expectedDetails)
        {
            var sut = new BaseErrorResponse(expectedStatusCode, expectedMessage, actualDetails);

            sut.Details.Should().Be(expectedDetails);
            sut.Message.Should().Be(expectedMessage);
            sut.StatusCode.Should().Be(expectedStatusCode);
        }
    }
}
