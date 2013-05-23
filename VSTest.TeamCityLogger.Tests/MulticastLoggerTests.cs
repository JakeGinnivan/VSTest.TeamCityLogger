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

            logger.Initialize(Substitute.For<TestLoggerEvents>(), new Dictionary<string, string>
            {
                {"logger1", "Test"},
                {"logger1.parameter", "some parameter"}
            });

            Assert.True(TestLogger.Parameters.ContainsKey("parameter"));
        }

        [ExtensionUri("logger://TestLogger")]
        [FriendlyName("Test")]
        class TestLogger : ITestLoggerWithParameters
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