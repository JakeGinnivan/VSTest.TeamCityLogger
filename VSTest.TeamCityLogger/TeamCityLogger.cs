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
        private static readonly string X = '\u0085'.ToString();
        private static readonly string L = '\u2029'.ToString();
        private static readonly string P = '\u2028'.ToString();
        private string _currentAssembly;

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
            try
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
            catch (Exception ex)
            {
                Console.WriteLine("##teamcity[message text='TeamCity Logger Error' errorDetails='{0}' status='ERROR']", FormatForTeamCity(ex.ToString()));
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
                        Console.WriteLine("##teamcity[testSuiteFinished name='{0}']", _currentAssembly);

                    _currentAssembly = currentAssembly;

                    Console.WriteLine("##teamcity[testSuiteStarted name='{0}']", currentAssembly);
                }

                string name = e.Result.TestCase.FullyQualifiedName;

                Console.WriteLine("##teamcity[testStarted name='{0}' captureStandardOutput='true']", name);

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
            catch (Exception ex)
            {
                Console.WriteLine("##teamcity[message text='TeamCity Logger Error' errorDetails='{0}' status='ERROR']", FormatForTeamCity(ex.ToString()));
            }
        }

        private static string FormatForTeamCity(string errorStackTrace)
        {
            if (errorStackTrace == null)
                return null;

            return errorStackTrace
                .Replace("|", "||")
                .Replace("\r", "|r")
                .Replace("\n", "|n")
                .Replace("'", "|'")
                .Replace(X, "|x")
                .Replace(L, "|l")
                .Replace(P, "|p")
                .Replace("[", "|[")
                .Replace("]", "|]");
        }

        /// <summary>
        /// Called when a test run is completed.
        /// </summary>
        private void TestRunCompleteHandler(object sender, TestRunCompleteEventArgs e)
        {
            Console.WriteLine("##teamcity[testSuiteFinished name='{0}']", _currentAssembly);
            Console.WriteLine("##teamcity[testSuiteFinished name='VSTest']");

            Console.WriteLine("Total Executed: {0}", e.TestRunStatistics.ExecutedTests);
            Console.WriteLine("Total Passed: {0}", e.TestRunStatistics[TestOutcome.Passed]);
            Console.WriteLine("Total Failed: {0}", e.TestRunStatistics[TestOutcome.Failed]);
            Console.WriteLine("Total Skipped: {0}", e.TestRunStatistics[TestOutcome.Skipped]);
        }
    }
}
