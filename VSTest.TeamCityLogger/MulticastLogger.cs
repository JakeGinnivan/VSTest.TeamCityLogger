using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace VSTest.TeamCityLogger
{
    [ExtensionUri("logger://MulticastLogger")]
    [FriendlyName("Multicast")]
    public class MulticastLogger : ITestLoggerWithParameters
    {
        private readonly PossibleLogger[] _loggers;

        public MulticastLogger()
        {
            var loggerType = typeof(ITestLogger);
            _loggers = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from typeInAssembly in assembly.GetTypes()
                        where loggerType.IsAssignableFrom(typeInAssembly)
                        select new PossibleLogger(assembly, typeInAssembly))
                    .ToArray();
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            throw new InvalidOperationException("This should never happen, but then again, vstest loggers are pretty much undocumented, so let me know if this happens!");
        }

        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
        {
            var loggers = parameters.Where(p => p.Key.StartsWith("logger", StringComparison.InvariantCultureIgnoreCase) && !p.Key.Contains("."));

            foreach (var keyValuePair in loggers)
            {
                var loggerType = _loggers.FirstOrDefault(l => FriendlyNameMatches(l, keyValuePair.Value) || LoggerUrlMatches(l, keyValuePair.Value));
                if (loggerType == null)
                {
                    throw new Exception(string.Format("Unable to find logger {0}", keyValuePair.Value));
                }
                var loggerInstance = (ITestLogger)Activator.CreateInstance(loggerType.TypeInAssembly, true);
                var loggerWithParameters = loggerInstance as ITestLoggerWithParameters;

                if (loggerWithParameters != null)
                {
                    var parametersFor = GetParametersFor(keyValuePair.Key, parameters);
                    loggerWithParameters.Initialize(events, parametersFor);
                }
                else
                {
                    loggerInstance.Initialize(events, parameters["TestRunDirectory"]);
                }
            }
        }

        private bool LoggerUrlMatches(PossibleLogger l, string loggerName)
        {
            return l.ExtensionUri != null && string.Equals(l.ExtensionUri.ExtensionUri, loggerName, StringComparison.InvariantCultureIgnoreCase);            
        }

        private static bool FriendlyNameMatches(PossibleLogger l, string loggerName)
        {
            return l.FriendlyName != null && string.Equals(l.FriendlyName.FriendlyName, loggerName, StringComparison.InvariantCultureIgnoreCase);
        }

        private Dictionary<string, string> GetParametersFor(string loggerName, Dictionary<string, string> parameters)
        {
            return (from parameter in parameters
                where parameter.Key.StartsWith(loggerName) && parameter.Key != loggerName
                let loggerParameter = parameter.Key.Split('.')[1]
                select new
                {
                    parameter,
                    loggerParameter
                }).ToDictionary(k => k.loggerParameter, k => k.parameter.Value);
        }
    }
}