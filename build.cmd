@echo Off
set target=%1
if "%target%" == "" (
   set target=BuildCmd
)
set config=%2
if "%config%" == "" (
   set config=Debug
)
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Build\Build.proj /t:"%target%" /p:Configuration="%config%" /m /fl /flp:LogFile=msbuild.log;Verbosity=Normal;Encoding=UTF-8 /nr:false