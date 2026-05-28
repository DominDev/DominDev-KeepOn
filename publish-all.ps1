Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

& "$PSScriptRoot\publish-portable-self-contained.ps1"
& "$PSScriptRoot\publish-framework-dependent.ps1"
& "$PSScriptRoot\publish-portable-compressed.ps1"
