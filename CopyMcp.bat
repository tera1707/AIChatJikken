@set @dummy=0/*
@echo off
NET FILE 1>NUL 2>NUL
if "%ERRORLEVEL%" neq "0" (
  cscript //nologo //E:JScript "%~f0" %*
  exit /b %ERRORLEVEL%
)

REM 管理者権限で実行したい処理 ここから

cd %~dp0

rem MCPサーバーを所定の位置にコピーする
xcopy ".\MyMcpServer\bin\x64\Debug\net9.0-windows10.0.26100.0\win-x64\" "C:\Program Files\MyMcpServer\" /y /d

pause
REM 管理者権限で実行したい処理 ここまで

goto :EOF
*/
var cmd = '"/c ""' + WScript.ScriptFullName + '" ';
for (var i = 0; i < WScript.Arguments.Length; i++) cmd += '"' + WScript.Arguments(i) + '" ';
(new ActiveXObject('Shell.Application')).ShellExecute('cmd.exe', cmd + ' "', '', 'runas', 1);
