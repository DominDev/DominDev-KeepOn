Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$publishRoot = "$PSScriptRoot\artifacts\publish"
if (Test-Path -LiteralPath $publishRoot) {
    Remove-Item -LiteralPath $publishRoot -Recurse -Force
}

& "$PSScriptRoot\publish-portable-self-contained.ps1"
& "$PSScriptRoot\publish-framework-dependent.ps1"
& "$PSScriptRoot\write-checksums.ps1"
