param(
  [Parameter(Mandatory = $true)]
  [string]$HostName,

  [Parameter(Mandatory = $true)]
  [string]$KeyPath,

  [string]$UserName = "ubuntu",
  [string]$RemotePath = "/opt/chatapp-demo",
  [string]$JwtSigningKey = "change-this-demo-signing-key-at-least-32-characters",
  [string]$PostgresPassword = "chatapp_demo_password"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$apiBaseUrl = "http://${HostName}:5185"
$frontendOrigin = "http://${HostName}"
$archiveName = "chatapp-demo-src.tar.gz"
$archivePath = Join-Path $env:TEMP ("chatapp-demo-{0}.tar.gz" -f (Get-Date -Format "yyyyMMddHHmmss"))
$remoteScriptName = "chatapp-demo-deploy-{0}.sh" -f (Get-Date -Format "yyyyMMddHHmmss")
$remoteScriptPath = "/tmp/$remoteScriptName"
$localScriptPath = Join-Path $env:TEMP $remoteScriptName

Write-Host "Creating deploy archive ..."
Push-Location $repoRoot
try {
  tar `
    --exclude ".git" `
    --exclude "ChatApp.UI/node_modules" `
    --exclude "ChatApp.UI/dist" `
    --exclude "ChatApp.API/bin" `
    --exclude "ChatApp.API/obj" `
    --exclude "ChatApp.API/wwwroot/uploads" `
    -czf $archivePath `
    ChatApp.API ChatApp.UI docker-compose.yml docker-compose.prod.yml .dockerignore
}
finally {
  Pop-Location
}

Write-Host "Uploading project to ${UserName}@${HostName}:$RemotePath ..."

ssh -i $KeyPath "$UserName@$HostName" "sudo mkdir -p $RemotePath && sudo chown -R $UserName $RemotePath"

scp -i $KeyPath $archivePath "${UserName}@${HostName}:/tmp/$archiveName"

$remoteCommand = @"
set -euxo pipefail
mkdir -p $RemotePath
rm -rf $RemotePath/ChatApp.API $RemotePath/ChatApp.UI $RemotePath/docker-compose.yml $RemotePath/.dockerignore
tar -xzf /tmp/$archiveName -C $RemotePath
rm -f /tmp/$archiveName
cd $RemotePath
export FRONTEND_PORT=80
export API_PORT=5185
export POSTGRES_PORT=5433
export VITE_API_BASE_URL=$apiBaseUrl
export FRONTEND_ORIGIN=$frontendOrigin
export JWT_SIGNING_KEY='$JwtSigningKey'
export POSTGRES_PASSWORD='$PostgresPassword'
docker compose -f docker-compose.prod.yml up -d --build
"@

Write-Host "Starting Docker Compose on EC2 ..."
[System.IO.File]::WriteAllText($localScriptPath, ($remoteCommand -replace "`r", ""), [System.Text.UTF8Encoding]::new($false))
scp -i $KeyPath $localScriptPath "${UserName}@${HostName}:$remoteScriptPath"
ssh -i $KeyPath "$UserName@$HostName" "bash $remoteScriptPath && rm -f $remoteScriptPath"

Remove-Item -LiteralPath $archivePath -Force
Remove-Item -LiteralPath $localScriptPath -Force

Write-Host ""
Write-Host "Frontend: http://$HostName"
Write-Host "API:      $apiBaseUrl"
