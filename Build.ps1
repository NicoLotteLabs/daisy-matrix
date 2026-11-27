$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(!(Test-Path $compiler)){ $compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
& $compiler /nologo /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:"$PSScriptRoot\DaisyMatrix.exe" "$PSScriptRoot\DaisyMatrix.cs"
if($LASTEXITCODE -ne 0){throw 'Compilation failed'}
