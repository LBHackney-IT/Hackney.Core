namespace Hackney.Core.Authorization.Exceptions
{
    public class EnvironmentVariableNullException : System.Exception
    {
        public EnvironmentVariableNullException(string variableName) : base($"Cannot resolve {variableName} environment variable.") { }

    }
}