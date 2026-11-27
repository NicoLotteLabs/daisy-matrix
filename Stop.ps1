New-Item -ItemType File -Force "$PSScriptRoot\stop.request" | Out-Null
Write-Host 'Stop requested. The overlay restores the original console color on normal exit.'
