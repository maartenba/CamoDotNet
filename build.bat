@echo Off
set config=%1
if "%config%" == "" (
    set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

if ""%msbuild%"" == "" (
    if exist "%programfiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
        set msbuild="%programfiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
    )
	if exist "%programfiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" (
        set msbuild="%programfiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
    )
	if exist "%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" (
        set msbuild="%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
    )
)
if ""%msbuild%"" == "" (
    echo Could not find suitable msbuild version
	goto failure
)

REM (optional) build.bat is in the root of our repo, cd to the correct folder where sources/projects are
cd src

REM Restore
call dotnet restore
if not "%errorlevel%"=="0" goto failure

REM Build
REM - Option 1: Run dotnet build for every source folder in the project
REM   e.g. call dotnet build <path> --configuration %config%
REM - Option 2: Let msbuild handle things and build the solution
call %msbuild% CamoDotNet.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
REM call dotnet build --configuration %config%
if not "%errorlevel%"=="0" goto failure

REM Unit tests
call dotnet test CamoDotNet.Tests --configuration %config%
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir %cd%\..\artifacts
call dotnet pack CamoDotNet --configuration %config% %version% --output ..\artifacts
call dotnet pack CamoDotNet.Core --configuration %config% %version% --output ..\artifacts
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1