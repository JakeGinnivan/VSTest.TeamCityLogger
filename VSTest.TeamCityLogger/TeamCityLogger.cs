using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace VSTest.TeamCityLogger
{
    [ExtensionUri("logger://TeamCityLogger")]
    [FriendlyName("TeamCityLogger")]
    public class TeamCityLogger : ITestLogger
    {
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

            Console.WriteLine("##teamcity[testSuiteStarted name='suite.name']");
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
            string name = e.Result.TestCase.FullyQualifiedName;

            Console.WriteLine("##teamcity[testStarted name='{0}']", name);

            if (e.Result.Outcome == TestOutcome.Skipped)
            {
                Console.WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", name, e.Result.ErrorMessage);
            }
            else if (e.Result.Outcome == TestOutcome.Failed)
            {
                Console.WriteLine("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", name, e.Result.ErrorMessage, e.Result.ErrorStackTrace);
            }
            else if (e.Result.Outcome == TestOutcome.Passed)
            {
                //// do nothing
            }

            Console.WriteLine("##teamcity[testFinished name='{0}' duration='{1}']", name, e.Result.Duration.TotalMilliseconds);
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

            Console.WriteLine("##teamcity[testSuiteFinished name='suite.name']");
        }
    }
}
