@echo Off
set config=%1
if "%config%" == "" (
    set config=Release
)

set version=
if not "%BuildCounter%" == "" (
   set packversionsuffix=--version-suffix ci-%BuildCounter%
)

REM Install .NET Core
call powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -Channel LTS"

REM (optional) build.bat is in the root of our repo, cd to the correct folder where sources/projects are
cd src

REM Restore
call dotnet restore
if not "%errorlevel%"=="0" goto failure

REM Build
call dotnet build CamoDotNet.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
call dotnet test CamoDotNet.Tests\CamoDotNet.Tests.csproj --configuration %config% --no-build
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir %cd%\..\artifacts
call dotnet pack CamoDotNet --configuration %config% %packversionsuffix% --output %cd%\..\artifacts
call dotnet pack CamoDotNet.Core --configuration %config% %packversionsuffix% --output %cd%\..\artifacts
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1
