#!/usr/bin/env pwsh
# Regenerates Kick.Client.Generated from the local OpenAPI spec.
# Requires: dotnet tool install -g Microsoft.OpenApi.Kiota (kiota)
# The Kick API swagger is Swagger 2.0 and must be converted to OpenAPI 3.x first.
# Prerequisites: npm install -g swagger2openapi

param(
    [string]$SpecPath = "$PSScriptRoot/../openapi/kick-swagger.yaml",
    [string]$OutputDir = "$PSScriptRoot/../src/Kick.Client.Generated/Generated"
)

$SpecPath = Resolve-Path $SpecPath
$openApiPath = [System.IO.Path]::ChangeExtension($SpecPath, ".openapi3.yaml")

Write-Host "Converting Swagger 2.0 → OpenAPI 3.0..." -ForegroundColor Cyan
swagger2openapi $SpecPath --outfile $openApiPath --yaml

Write-Host "Running Kiota code generation..." -ForegroundColor Cyan
kiota generate `
    --language csharp `
    --openapi $openApiPath `
    --class-name KickApiClient `
    --namespace-name "Kick.Client.Generated" `
    --output $OutputDir `
    --clean-output

Write-Host "Done. Review changes in $OutputDir" -ForegroundColor Green
