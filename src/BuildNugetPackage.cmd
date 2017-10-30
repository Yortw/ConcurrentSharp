del /F /Q /S *.CodeAnalysisLog.xml

"..\.nuget\NuGet.exe" pack -sym ConcurrentSharp.nuspec -BasePath .\
pause

copy *.nupkg C:\Nuget.LocalRepository\
pause
