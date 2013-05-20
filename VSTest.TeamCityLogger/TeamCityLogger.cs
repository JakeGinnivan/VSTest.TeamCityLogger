using System;
using System.Diagnostics;
using System.IO;
using JetBrains.TeamCity.ServiceMessages.Write.Special;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace VSTest.TeamCityLogger
{
    [ExtensionUri("logger://TeamCityLogger")]
    [FriendlyName("TeamCityLogger")]
    public class TeamCityLogger : ITestLogger
    {
        private string _currentAssembly;
        private ITeamCityWriter _teamCityWriter;
        private ITeamCityTestsSubWriter _vsTestSuite;
        private ITeamCityTestsSubWriter _currentAssemblySuite;
        private ITeamCityTestWriter _currentTest;

        public TeamCityLogger()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        /// <summary>
        /// Initializes the Test Logger.
        /// </summary>
        /// <param name="events">Events that can be registered for.</param>
        /// <param name="testRunDirectory">Test Run Directory</param>
        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            // Register for the events.
            events.TestRunMessage += TestMessageHandler;
            events.TestResult += TestResultHandler;
            events.TestRunComplete += TestRunCompleteHandler;

            _teamCityWriter = new TeamCityServiceMessages().CreateWriter(w => Trace.WriteLine(w));
            _vsTestSuite = _teamCityWriter.OpenTestSuite("VSTest");
        }

        /// <summary>
        /// Called when a test message is received.
        /// </summary>
        private void TestMessageHandler(object sender, TestRunMessageEventArgs e)
        {
            try
            {
                switch (e.Level)
                {
                    case TestMessageLevel.Informational:
                    case TestMessageLevel.Warning:
                        _currentTest.WriteStdOutput(e.Message);
                        break;

                    case TestMessageLevel.Error:
                        _currentTest.WriteErrOutput(e.Message);
                        break;
                }
            }
            catch (Exception ex)
            {
                _teamCityWriter.WriteError("TeamCity Logger Error", ex.ToString());
            }
        }

        /// <summary>
        /// Called when a test result is received.
        /// </summary>
        private void TestResultHandler(object sender, TestResultEventArgs e)
        {
            try
            {
                var currentAssembly = Path.GetFileName(e.Result.TestCase.Source);
                if (_currentAssembly != currentAssembly)
                {
                    if (!string.IsNullOrEmpty(_currentAssembly))
                        _currentAssemblySuite.Dispose();

                    _currentAssembly = currentAssembly;
                    _currentAssemblySuite = _vsTestSuite.OpenTestSuite(_currentAssembly);
                }

                using (_currentTest = _currentAssemblySuite.OpenTest(e.Result.TestCase.FullyQualifiedName))
                {
                    if (e.Result.Outcome == TestOutcome.Skipped)
                    {
                        _currentTest.WriteIgnored(e.Result.ErrorMessage);
                    }
                    else if (e.Result.Outcome == TestOutcome.Failed)
                    {
                        _currentTest.WriteFailed(e.Result.ErrorMessage, e.Result.ErrorStackTrace);
                    }
                    _currentTest.WriteDuration(e.Result.Duration);
                }
            }
            catch (Exception ex)
            {
                _teamCityWriter.WriteError("TeamCity Logger Error", ex.ToString());
            }
        }

        /// <summary>
        /// Called when a test run is completed.
        /// </summary>
        private void TestRunCompleteHandler(object sender, TestRunCompleteEventArgs e)
        {
            if (_currentAssemblySuite != null) _currentAssemblySuite.Dispose();
            _vsTestSuite.Dispose();

            _teamCityWriter.Dispose();

            Trace.WriteLine(string.Format("Total Executed: {0}", e.TestRunStatistics.ExecutedTests));
            Trace.WriteLine(string.Format("Total Passed: {0}", e.TestRunStatistics[TestOutcome.Passed]));
            Trace.WriteLine(string.Format("Total Failed: {0}", e.TestRunStatistics[TestOutcome.Failed]));
            Trace.WriteLine(string.Format("Total Skipped: {0}", e.TestRunStatistics[TestOutcome.Skipped]));
        }
    }
}
