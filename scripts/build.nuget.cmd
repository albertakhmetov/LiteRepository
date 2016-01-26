rd ..\binaries\ /s /q
md ..\binaries\

set config=Release

..\nuget.exe restore ..\LiteRepository.sln
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=%config% ..\LiteRepository.sln

..\nuget.exe pack "..\sources\LiteRepository\LiteRepository.csproj" -IncludeReferencedProjects -OutputDirectory ..\binaries -Prop Configuration=%config%