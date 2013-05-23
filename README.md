VSTest.TeamCityLogger
=====================

Enables TeamCity to display output when tests are run through VSTest.console.exe

**Note:** VSTest.TeamCityLogger needs at least VS2012.1 (Visual Studio 2012 Update 1) installed

#Usage

Put VSTest.TeamCityLogger into `C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\Extensions`

    vstest.console.exe tests.dll /logger:TeamCity

If you would like to use another logger in conjunction with the TeamCityLogger you can use the MulticastLogger

    vstest.console.exe tests.dll /logger:Multicast;logger1=TeamCity;logger2=TfsPublisher;logger2.Collection=<team project url>;logger2.BuildName=<build name>;logger2.TeamProject=<team project name>;

The convention is that you use logger1, logger2 etc, then if that logger takes parameters, use LoggerKey.LoggerParameter. i.e logger1.AParameter, or logger2.SomeParameter

## Download
You can grab the extension straight off the build server:

[CI Build](http://teamcity.ginnivan.net/viewType.html?buildTypeId=bt47)  
[Latest Build Output (VSTest.TeamCityLogger.dll)](http://teamcity.ginnivan.net/repository/download/bt47/.lastSuccessful/VSTest.TeamCityLogger.dll)