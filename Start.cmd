@echo off
setlocal
set "DAISYMATRIX_ROOT=%~dp0"
if exist "%~dp0stop.request" del "%~dp0stop.request"
if not "%~1"=="" (
  start "" "%~dp0DaisyMatrix.exe" "%~1"
  exit /b
)
powershell -NoProfile -Command "$a=@(Get-CimInstance Win32_Process); foreach($n in ($a | Where-Object {$_.Name -eq 'node.exe' -and $_.CommandLine -match 'desktop-commander'})){ $p=$n; while($p){$ui=Get-Process -Id $p.ProcessId -ErrorAction SilentlyContinue; if($p.Name -eq 'cmd.exe' -and $ui.MainWindowHandle -ne 0){Start-Process (Join-Path $env:DAISYMATRIX_ROOT 'DaisyMatrix.exe') -ArgumentList ([string]$p.ProcessId); exit 0};$parent=$p.ParentProcessId;$p=$a | Where-Object {$_.ProcessId -eq $parent} | Select-Object -First 1}}; Write-Host 'Console not found. Use Start.cmd PID'; exit 1"
if errorlevel 1 pause
