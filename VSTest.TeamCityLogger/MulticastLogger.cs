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
        internal const string Testrundirectory = "TestRunDirectory";
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
            var loggersParameter = parameters.Keys.SingleOrDefault(k => string.Equals("loggers", k, StringComparison.InvariantCultureIgnoreCase));
            var loggers = loggersParameter == null ?
                parameters.Where(p => !p.Key.Contains(".") && p.Key != Testrundirectory) :
                parameters[loggersParameter].Split(',').Select(l => new KeyValuePair<string, string>(l, l));

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
                    loggerInstance.Initialize(events, parameters[Testrundirectory]);
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
            var parametersForLogger = (from parameter in parameters
                                       where parameter.Key.Contains(".") && parameter.Key.Split('.')[0] == loggerName
                                       let loggerParameter = parameter.Key.Split('.')[1]
                                       select new
                                       {
                                           key = loggerParameter,
                                           value = parameter.Value
                                       }).ToDictionary(k => k.key, k => k.value);
            parametersForLogger.Add(Testrundirectory, parameters[Testrundirectory]);
            return parametersForLogger;
        }
    }
}