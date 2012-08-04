@echo off

start /wait notepad %~dp0src\UITextBlockControl\UITextBlockControl.nuspec
%~dp0src\.nuget\nuget.exe pack %~dp0src\UITextBlockControl\UITextBlockControl.csproj -build

pause