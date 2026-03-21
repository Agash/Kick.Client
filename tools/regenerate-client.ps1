#!/usr/bin/env pwsh
# Regenerates Kick.Client.Generated from the local OpenAPI spec.
# Prerequisites:
#   npm install -g swagger2openapi
#   dotnet tool install -g Microsoft.OpenApi.Kiota

param(
    [string]$SpecPath = "$PSScriptRoot/../openapi/kick-swagger.yaml",
    [string]$OutputDir = "$PSScriptRoot/../src/Kick.Client.Generated/Generated"
)

$SpecPath = Resolve-Path $SpecPath
$OpenApiPath = [System.IO.Path]::ChangeExtension($SpecPath, ".openapi3.yaml")

Write-Host "Converting Swagger 2.0 to OpenAPI 3.0..." -ForegroundColor Cyan
swagger2openapi $SpecPath --outfile $OpenApiPath --yaml

Write-Host "Running Kiota code generation..." -ForegroundColor Cyan
kiota generate `
    --language csharp `
    --openapi $OpenApiPath `
    --class-name KickApiClient `
    --namespace-name "Kick.Client.Generated" `
    --output $OutputDir `
    --clean-output

Write-Host "Done. Review changes in $OutputDir" -ForegroundColor Green
