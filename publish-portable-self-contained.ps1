Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$output = "$PSScriptRoot\artifacts\publish\portable-self-contained"
if (Test-Path -LiteralPath $output) {
    Remove-Item -LiteralPath $output -Recurse -Force
}

dotnet publish "$PSScriptRoot\KeepOn.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $output `
    /p:PublishSingleFile=true
