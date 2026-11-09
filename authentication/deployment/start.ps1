$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$api = Join-Path $root 'GlobalSuccess.Api\GlobalSuccess.Api.csproj'
$desktop = Join-Path $root 'GlobalSuccess.Desktop\GlobalSuccess.Desktop.csproj'
if (!(Test-Path $api) -or !(Test-Path $desktop)) { throw 'Khong tim thay project API hoac Desktop.' }
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Chua cai dotnet SDK.' }
Write-Host 'Dang khoi dong Backend API...'
$apiProc = Start-Process -FilePath 'dotnet' -ArgumentList @('run','--project',('"'+$api+'"')) -PassThru
$ready = $false
for ($i=0; $i -lt 60; $i++) {
  if ($apiProc.HasExited) { throw 'Backend da dung. Kiem tra loi bang dotnet run --project GlobalSuccess.Api\GlobalSuccess.Api.csproj' }
  try {
    $r = Invoke-WebRequest -Uri 'http://localhost:5116/' -TimeoutSec 2 -UseBasicParsing
    $ready = $true; break
  } catch {
    if ($_.Exception.Response) { $ready = $true; break }
  }
  Start-Sleep -Seconds 1
}
if (!$ready) { throw 'API khong phan hoi tai localhost:5116 sau 60 giay.' }
Write-Host 'API da san sang. Dang mo WPF...'
Start-Process -FilePath 'dotnet' -ArgumentList @('run','--project',('"'+$desktop+'"'))
Write-Host 'Da gui lenh khoi dong WPF. Backend PID:' $apiProc.Id
