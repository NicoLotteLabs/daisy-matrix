param([int]$ConsolePid=0)
$ErrorActionPreference='Stop'
if(Get-Process DaisyMatrix -ErrorAction SilentlyContinue){throw 'DaisyMatrix already running.'}
if(!(Test-Path "$PSScriptRoot\DaisyMatrix.exe")){& "$PSScriptRoot\Build.ps1"}
if($ConsolePid -eq 0){
 $all=@(Get-CimInstance Win32_Process)
 $nodes=@($all | Where-Object {$_.Name -eq 'node.exe' -and $_.CommandLine -match 'desktop-commander'})
 foreach($node in $nodes){
  $proc=$node
  while($proc){
   $ui=Get-Process -Id $proc.ProcessId -ErrorAction SilentlyContinue
   if($proc.Name -eq 'cmd.exe' -and $ui -and $ui.MainWindowHandle -ne 0){$ConsolePid=$proc.ProcessId;break}
   $parent=$proc.ParentProcessId
   $proc=$all | Where-Object {$_.ProcessId -eq $parent} | Select-Object -First 1
  }
  if($ConsolePid){break}
 }
}
if(!$ConsolePid){throw 'Classic Desktop Commander console not found. Specify -ConsolePid manually.'}
Remove-Item "$PSScriptRoot\stop.request" -ErrorAction SilentlyContinue
Start-Process "$PSScriptRoot\DaisyMatrix.exe" -ArgumentList ([string]$ConsolePid)
Write-Host "Daisy Matrix launched for console PID $ConsolePid"
