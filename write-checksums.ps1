Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$publishRoot = "$PSScriptRoot\artifacts\publish"
$checksumPath = Join-Path $publishRoot 'SHA256SUMS.txt'

if (-not (Test-Path -LiteralPath $publishRoot)) {
    throw "Publish directory does not exist: $publishRoot"
}

$files = Get-ChildItem -LiteralPath $publishRoot -Recurse -File |
    Where-Object { $_.Name -ne 'SHA256SUMS.txt' } |
    Sort-Object FullName

$lines = foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($publishRoot.Length).TrimStart('\').Replace('\', '/')
    $hash = Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256
    "$($hash.Hash.ToLowerInvariant())  $relativePath"
}

Set-Content -LiteralPath $checksumPath -Value $lines -Encoding utf8
