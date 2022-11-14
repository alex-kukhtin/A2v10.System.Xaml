del /q A2v10.System.Xaml\bin\Release\*.nupkg
del /q A2v10.System.Xaml\bin\Release\*.snupkg

dotnet build -c Release

del /q ..\NuGet.local\*.*

copy A2v10.System.Xaml\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.System.Xaml\bin\Release\*.snupkg ..\NuGet.local

pause