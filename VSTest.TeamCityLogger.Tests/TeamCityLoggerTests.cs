using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NSubstitute;
using Xunit;

namespace VSTest.TeamCityLogger.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class TeamCityLoggerTests
    {
        [Fact]
        public void SimpleScenario()
        {
            var sb = new StringBuilder();
            Trace.Listeners.Clear();
            var logger = new TeamCityLogger();
            Trace.Listeners.Add(new TextWriterTraceListener(new StringWriter(sb)));
            var events = Substitute.For<TestLoggerEvents>();
            logger.Initialize(events, string.Empty);

            events.TestResult +=
                Raise.EventWith(
                    new TestResultEventArgs(
                        new TestResult(new TestCase("Test.FullyQualified.Name", new Uri("logger://TeamCityLogger"),
                            "c:\\TestAssembly1.dll"))));
            events.TestResult +=
                Raise.EventWith(
                    new TestResultEventArgs(
                        new TestResult(new TestCase("Test.FullyQualified.Name2", new Uri("logger://TeamCityLogger"),
                            "c:\\TestAssembly2.dll"))));

            events.TestRunComplete +=
                Raise.EventWith(
                    new TestRunCompleteEventArgs(Substitute.For<ITestRunStatistics>(), false, false, null,
                        new Collection<AttachmentSet>(), TimeSpan.FromSeconds(20)));

            Approvals.Verify(Regex.Replace(Regex.Replace(sb.ToString(), " timestamp='[^']*'", string.Empty), " flowId='[^']*'", string.Empty));
        }
    }
}
