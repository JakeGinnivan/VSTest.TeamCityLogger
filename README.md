VSTest.TeamCityLogger
=====================

Enables TeamCity to display output when tests are run through VSTest.console.exe

#Usage

Put VSTest.TeamCityLogger into `C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\Extensions`

    vstest.console.exe tests.dll /logger:TeamCityLogger

or if you want a .trx file as well

    vstest.console.exe tests.dll /logger:TeamCityAndTrxLogger

## Download
You can grab the extension straight off the build server:

[CI Build](http://teamcity.ginnivan.net/viewType.html?buildTypeId=bt47)  
[Latest Build Output (VSTest.TeamCityLogger.dll)](http://teamcity.ginnivan.net/repository/download/bt47/.lastSuccessful/VSTest.TeamCityLogger.dll)