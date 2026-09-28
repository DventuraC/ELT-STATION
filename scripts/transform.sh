#!/usr/bin/env bash
set -euo pipefail
dotnet "${WORKER_DLL:-./src/StationSales.Worker/bin/Release/net6.0/StationSales.Worker.dll}" transform "$@"
