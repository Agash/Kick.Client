#!/usr/bin/env bash
# Regenerates Kick.Client.Generated from the local OpenAPI spec.
# Requires: kiota (dotnet tool install -g Microsoft.OpenApi.Kiota)
# Also requires swagger2openapi: npm install -g swagger2openapi

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SPEC_PATH="${1:-$SCRIPT_DIR/../openapi/kick-swagger.yaml}"
OPENAPI_PATH="${SPEC_PATH%.yaml}.openapi3.yaml"
OUTPUT_DIR="$SCRIPT_DIR/../src/Kick.Client.Generated/Generated"

echo "Converting Swagger 2.0 → OpenAPI 3.0..."
swagger2openapi "$SPEC_PATH" --outfile "$OPENAPI_PATH" --yaml

echo "Running Kiota code generation..."
kiota generate \
    --language csharp \
    --openapi "$OPENAPI_PATH" \
    --class-name KickApiClient \
    --namespace-name "Kick.Client.Generated" \
    --output "$OUTPUT_DIR" \
    --clean-output

echo "Done. Review changes in $OUTPUT_DIR"
