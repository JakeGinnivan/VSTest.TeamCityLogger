using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace VSTest.TeamCityLogger
{
    [ExtensionUri("logger://TeamCityLogger")]
    [FriendlyName("TeamCityLogger")]
    public class TeamCityLogger : ITestLogger
    {
        private readonly string _x;
        private readonly string _l;
        private readonly string _p;
        private string _currentAssembly;

        public TeamCityLogger()
        {
            _x = '\u0085'.ToString();
            _p = '\u2029'.ToString();
            _l = '\u2028'.ToString();
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

            Console.WriteLine("##teamcity[testSuiteStarted name='VSTest']");
        }

        /// <summary>
        /// Called when a test message is received.
        /// </summary>
        private static void TestMessageHandler(object sender, TestRunMessageEventArgs e)
        {
            switch (e.Level)
            {
                case TestMessageLevel.Informational:
                    Console.WriteLine("Information: " + e.Message);
                    break;

                case TestMessageLevel.Warning:
                    Console.WriteLine("Warning: " + e.Message);
                    break;

                case TestMessageLevel.Error:
                    Console.WriteLine("Error: " + e.Message);
                    break;
            }
        }

        /// <summary>
        /// Called when a test result is received.
        /// </summary>
        private void TestResultHandler(object sender, TestResultEventArgs e)
        {
            var currentAssembly = Path.GetFileName(e.Result.TestCase.Source);
            if (_currentAssembly != currentAssembly)
            {
                if (!string.IsNullOrEmpty(_currentAssembly))
                    Console.WriteLine("##teamcity[testSuiteFinished name='{0}']", _currentAssembly);

                _currentAssembly = currentAssembly;

                Console.WriteLine("##teamcity[testSuiteStarted name='{0}']", currentAssembly);
            }

            string name = e.Result.TestCase.FullyQualifiedName.Replace(e.Result.TestCase.DisplayName, e.Result.DisplayName);

            Console.WriteLine("##teamcity[testStarted name='{0}' captureStandardOutput='false']", name);

            if (e.Result.Outcome == TestOutcome.Skipped)
            {
                Console.WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", name, FormatForTeamCity(e.Result.ErrorMessage));
            }
            else if (e.Result.Outcome == TestOutcome.Failed)
            {
                var errorStackTrace = FormatForTeamCity(e.Result.ErrorStackTrace);
                Console.WriteLine("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", name, FormatForTeamCity(e.Result.ErrorMessage), errorStackTrace);
            }
            else if (e.Result.Outcome == TestOutcome.Passed)
            {
            }

            Console.WriteLine("##teamcity[testFinished name='{0}' duration='{1}']", name, e.Result.Duration.TotalMilliseconds);
        }

        private string FormatForTeamCity(string errorStackTrace)
        {
            return errorStackTrace
                .Replace("\r", "|r")
                .Replace("\n", "|n")
                .Replace("'", "|'")
                .Replace(_x, "|x")
                .Replace(_l, "|l")
                .Replace(_p, "|p")
                .Replace("|", "||")
                .Replace("[", "|[")
                .Replace("]", "|]");
        }

        /// <summary>
        /// Called when a test run is completed.
        /// </summary>
        private void TestRunCompleteHandler(object sender, TestRunCompleteEventArgs e)
        {
            Console.WriteLine("Total Executed: {0}", e.TestRunStatistics.ExecutedTests);
            Console.WriteLine("Total Passed: {0}", e.TestRunStatistics[TestOutcome.Passed]);
            Console.WriteLine("Total Failed: {0}", e.TestRunStatistics[TestOutcome.Failed]);
            Console.WriteLine("Total Skipped: {0}", e.TestRunStatistics[TestOutcome.Skipped]);

            Console.WriteLine("##teamcity[testSuiteFinished name='{0}']", _currentAssembly);
            Console.WriteLine("##teamcity[testSuiteFinished name='VSTest']");
        }
    }
}
