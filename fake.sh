#!/usr/bin/env bash

set -euo pipefail

FAKE_DETAILED_ERRORS=true dotnet run --project ./build/build.fsproj -- -t "$@"