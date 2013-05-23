using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NSubstitute;
using Xunit;

namespace VSTest.TeamCityLogger.Tests
{
    public class MulticastLoggerTests
    {
        [Fact]
        public void parses_parameters_correctly()
        {
            var logger = new MulticastLogger();

            const string someParameter = "some parameter";
            const string differentLoggerParameter = "different logger parameter";
            logger.Initialize(Substitute.For<TestLoggerEvents>(), new Dictionary<string, string>
            {
                {"logger1", "Test"},
                {"logger1.parameter", someParameter},
                {"logger2", "Test2"},
                {"logger2.parameter", differentLoggerParameter},
                {MulticastLogger.Testrundirectory, "c:\\TestRunDir"}
            });

            Assert.Equal(someParameter, TestLogger.Parameters["parameter"]);
            Assert.Equal(differentLoggerParameter, Test2Logger.Parameters["parameter"]);
        }

        [Fact]
        public void parses_alternate_syntax_parameters_correctly()
        {
            var logger = new MulticastLogger();

            const string someParameter = "some parameter";
            const string differentLoggerParameter = "different logger parameter";
            logger.Initialize(Substitute.For<TestLoggerEvents>(), new Dictionary<string, string>
            {
                {"loggers", "Test,Test2"},
                {"Test.parameter", someParameter},
                {"Test2.parameter", differentLoggerParameter},
                {MulticastLogger.Testrundirectory, "c:\\TestRunDir"}
            });

            Assert.Equal(someParameter, TestLogger.Parameters["parameter"]);
            Assert.Equal(differentLoggerParameter, Test2Logger.Parameters["parameter"]);
        }

        [Fact]
        public void passes_test_run_dir_to_other_loggers_with_parameters()
        {
            var logger = new MulticastLogger();
            const string runDir = "c:\\TestRunDir";

            logger.Initialize(Substitute.For<TestLoggerEvents>(), new Dictionary<string, string>
            {
                {"logger1", "Test"},
                {MulticastLogger.Testrundirectory, runDir}
            });

            Assert.True(TestLogger.Parameters.ContainsKey(MulticastLogger.Testrundirectory));
            Assert.Equal(runDir, TestLogger.Parameters[MulticastLogger.Testrundirectory]);
        }

        [ExtensionUri("logger://TestLogger")]
        [FriendlyName("Test")]
        internal class TestLogger : ITestLoggerWithParameters
        {
            public static Dictionary<string, string> Parameters;

            public void Initialize(TestLoggerEvents events, string testRunDirectory)
            {

            }

            public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
            {
                Parameters = parameters;
            }
        }

        [ExtensionUri("logger://Test2Logger")]
        [FriendlyName("Test2")]
        internal class Test2Logger : ITestLoggerWithParameters
        {
            public static Dictionary<string, string> Parameters;

            public void Initialize(TestLoggerEvents events, string testRunDirectory)
            {

            }

            public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
            {
                Parameters = parameters;
            }
        }
    }
}