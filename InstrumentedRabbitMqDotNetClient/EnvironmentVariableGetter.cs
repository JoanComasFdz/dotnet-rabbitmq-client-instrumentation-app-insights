using System;
using System.Collections.Generic;
using System.Linq;

namespace InstrumentedRabbitMqDotNetClient
{
    internal class EnvironmentVariableGetter
    {
        private const string NotFound = "%$#NOT_FOUND>>!";

        public static IDictionary<string, string> GetValues(params string[] variableNames)
        {
            var variablesValues = variableNames
                .ToDictionary(k => k, GetValueOrEmpty);
            var notFoundVars = variablesValues
                .Where(pair => pair.Value == NotFound)
                .Select(pair => pair.Key)
                .ToArray();

            if (notFoundVars.Length > 0)
            {
                var emptyVariablesText = string.Join(", ", notFoundVars);
                throw new ArgumentException($"The following environment variables cannot be obtained: {emptyVariablesText}");
            }

            return variablesValues;
        }

        private static string GetValueOrEmpty(string variableName)
        {
            try
            {
                var value = Environment.GetEnvironmentVariable(variableName);
                return string.IsNullOrWhiteSpace(value) ? NotFound : value;
            }
            catch
            {
                return NotFound;
            }
        }
    }
}
