using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace VSTest.TeamCityLogger
{
    [ExtensionUri("logger://TeamCityAndTrxLogger")]
    [FriendlyName("TeamCityAndTrxLogger")]
    public class TeamCityAndTrxLogger : ITestLogger
    {
        private readonly TeamCityLogger _teamCityLogger;
        private readonly ITestLogger _trxLogger;

        public TeamCityAndTrxLogger()
        {
            _teamCityLogger = new TeamCityLogger();
            var trxLoggerType = Type.GetType("Microsoft.VisualStudio.TestPlatform.Extensions.TrxLogger, Microsoft.VisualStudio.TestPlatform.Extensions.TrxLogger, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            _trxLogger = (ITestLogger)Activator.CreateInstance(trxLoggerType, true);
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            _teamCityLogger.Initialize(events, testRunDirectory);
            _trxLogger.Initialize(events, testRunDirectory);
        }
    }
}