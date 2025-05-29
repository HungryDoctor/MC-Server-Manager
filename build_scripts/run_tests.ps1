<#!  Build-free (optional) test runner
.SYNOPSIS
    Runs dotnet test on the AllProjects solution.

.DESCRIPTION
    * By default it restores, builds and tests.
    * Pass -NoBuild and it will call `dotnet test --no-build --no-restore`
      so it reuses what the build step already compiled.

.PARAMETER Configuration
    Debug | Release (defaults to Release).

.PARAMETER NoBuild
    Skip the build/restore inside dotnet test.

.EXAMPLE
    # Restore, build and run tests with Release configuration (default)
    ./run_tests.ps1

.EXAMPLE
    # Run tests in Debug without rebuild
    ./run_tests.ps1 -Configuration Debug -NoBuild
#>

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$NoBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$SolutionPath = Join-Path $PSScriptRoot -ChildPath "..\src\solutions\AllProjects\AllProjects.slnx"
if (-not (Test-Path $SolutionPath)) {
    Write-Error "Solution file not found: $SolutionPath"
    exit 1
}

Write-Host "Testing solution: $SolutionPath"
Write-Host "Configuration : $Configuration"
Write-Host ($NoBuild ? " (skipping build/restore)" : "")

$testArgs = @(
    "test", $SolutionPath,
    "-c",   $Configuration,
    "--logger", "trx"
)

if ($NoBuild)  { 
    $testArgs += "--no-build";
    $testArgs += "--no-restore" 
}

dotnet @testArgs
exit $LASTEXITCODE