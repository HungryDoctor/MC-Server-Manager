<#!
.SYNOPSIS
    Builds every project referenced by the AllProjects solution filter.

.DESCRIPTION
    * Restores NuGet packages (unless -NoRestore is passed)
    * Executes `dotnet build` with the chosen configuration and maximum CPU cores.
    * Exits with the same exit‑code as `dotnet build`, so CI can fail the job if the build fails.

.PARAMETER Configuration
    The MSBuild configuration to use (Debug | Release).  Defaults to Release.

.PARAMETER NoRestore
    Skip the explicit `dotnet restore` step.

.EXAMPLE
    # Restore & build with Release configuration (default)
    ./build_all.ps1

.EXAMPLE
    # Build in Debug without restoring first
    ./build_all.ps1 -Configuration Debug -NoRestore
#>

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$NoRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Resolve the solution path relative to this script’s location
$SolutionPath = Join-Path -Path $PSScriptRoot -ChildPath "..\dev\solutions\AllProjects\AllProjects.slnx"
if (-not (Test-Path $SolutionPath)) {
    Write-Error "Solution file not found: $SolutionPath"
    exit 1
}

Write-Host "Building solution: $SolutionPath"
Write-Host "Configuration: $Configuration"
if ($NoRestore) { 
    Write-Host " (restore skipped)" 
} else { 
    Write-Host
} 

# ------------------------------------------------------------
# Restore (optional)
# ------------------------------------------------------------
if (-not $NoRestore) {
    dotnet restore $SolutionPath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet restore failed with exit‑code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

# ------------------------------------------------------------
# Build
# ------------------------------------------------------------
$buildArgs = @("build", $SolutionPath, "-c", $Configuration)

dotnet @buildArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet build failed with exit‑code $LASTEXITCODE"
}

exit $LASTEXITCODE
