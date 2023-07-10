#!/usr/bin/env bash

dotnet tool restore
dotnet paket install
dotnet build
npm ci