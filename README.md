VSTest.TeamCityLogger
=====================

Enables TeamCity to display output when tests are run through VSTest.console.exe

**Note:** VSTest.TeamCityLogger needs at least VS2012.1 (Visual Studio 2012 Update 1) installed

#Usage

Put VSTest.TeamCityLogger into `C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\Extensions`

    vstest.console.exe tests.dll /logger:TeamCity

If you would like to use another logger in conjunction with the TeamCityLogger you can use the MulticastLogger!

Simply comma separate the loggers in `loggers` parameter, if any of the loggers have parameters you can use `LoggerName.ParameterName` to set the `ParameterName` parameter. For example:

    vstest.console.exe tests.dll /logger:Multicast;loggers=TeamCity,trx,TfsPublisher;TfsPublisher.Collection=<team project url>;TfsPublisher.BuildName=<build name>;TfsPublisher.TeamProject=<team project name>;

You may also alias loggers, then you can set parameters on the aliases:

    vstest.console.exe tests.dll /logger:Multicast;logger1=TeamCity;logger2=trx;logger3=TfsPublisher;logger3.Collection=<team project url>;logger3.BuildName=<build name>;logger3.TeamProject=<team project name>;

## Download
You can grab the extension straight off the build server:

[CI Build](http://teamcity.ginnivan.net/viewType.html?buildTypeId=bt47)  
[Latest Build Output (VSTest.TeamCityLogger.dll)](http://teamcity.ginnivan.net/repository/download/bt47/.lastSuccessful/VSTest.TeamCityLogger.dll)