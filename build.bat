@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

REM Package restore
tools\nuget.exe restore src\CamoDotNet.sln -OutputDirectory %cd%\src\packages -NonInteractive

REM Build
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild" src\CamoDotNet.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

REM Test
tools\nuget.exe install xunit.runner.console -Version 2.0.0 -OutputDirectory src\packages
src\packages\xunit.runner.console.2.0.0\tools\xunit.console.exe src\CamoDotNet.Tests\bin\%config%\CamoDotNet.Tests.dll

REM Package
mkdir artifacts
mkdir artifacts\nuget
tools\nuget.exe pack "src\CamoDotNet\CamoDotNet.csproj" -symbols -o artifacts\nuget -p Configuration=%config% %version%
tools\nuget.exe pack "src\CamoDotNet.Core\CamoDotNet.Core.csproj" -symbols -o artifacts\nuget -p Configuration=%config% %version%

REM Plain assemblies
mkdir artifacts\assemblies
copy src\CamoDotNet\bin\%config%\Camo*.dll artifacts\assemblies
copy src\CamoDotNet\bin\%config%\Camo*.pdb artifacts\assemblies
copy src\CamoDotNet.Core\bin\%config%\Camo*.dll artifacts\assemblies
copy src\CamoDotNet.Core\bin\%config%\Camo*.pdb artifacts\assemblies
