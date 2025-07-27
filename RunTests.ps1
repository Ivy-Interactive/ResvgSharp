# ResvgSharp Build and Test Script
# This script builds the Rust native library, .NET solution, and runs all tests

param(
    [string]$Configuration = "Release",
    [switch]$SkipNativeBuild,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Header {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "→ $Message" -ForegroundColor Yellow
}

# Check if running from correct directory
if (-not (Test-Path "ResvgSharp.sln")) {
    Write-Error "Please run this script from the ResvgSharp root directory"
    exit 1
}

Write-Header "ResvgSharp Build and Test"
Write-Host "Configuration: $Configuration"

try {
    
    # Build Rust native library
    if (-not $SkipNativeBuild) {
        Write-Header "Building Rust native library"
        
        Push-Location "native/resvg-wrapper"
        try {
            # Check if Rust is installed
            if (-not (Get-Command "cargo" -ErrorAction SilentlyContinue)) {
                Write-Error "Rust is not installed. Please install from https://rustup.rs/"
                exit 1
            }
            
            Write-Info "Building for current platform..."
            if ($Configuration -eq "Debug") {
                cargo build
            } else {
                cargo build --release
            }
            
            if ($LASTEXITCODE -ne 0) {
                throw "Rust build failed"
            }
            
            Write-Success "Rust native library built successfully"
            
            # Copy native library to expected location
            Write-Info "Copying native library to build directory..."
            
            $targetDir = if ($Configuration -eq "Debug") { "debug" } else { "release" }
            $sourceDir = "target/$targetDir"
            
            # Determine platform and library name
            $rid = ""
            $libName = ""
            if ($IsWindows -or $PSVersionTable.Platform -eq "Win32NT") {
                $rid = "win-x64"
                $libName = "resvg_wrapper.dll"
            } elseif ($IsMacOS) {
                if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq "Arm64") {
                    $rid = "osx-arm64"
                } else {
                    $rid = "osx-x64"
                }
                $libName = "libresvg_wrapper.dylib"
            } elseif ($IsLinux) {
                if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq "Arm64") {
                    $rid = "linux-arm64"
                } else {
                    $rid = "linux-x64"
                }
                $libName = "libresvg_wrapper.so"
            }
            
            $outputDir = "../../build/runtimes/$rid/native"
            New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
            
            Copy-Item "$sourceDir/$libName" "$outputDir/" -Force
            Write-Success "Native library copied to $outputDir"
            
        } finally {
            Pop-Location
        }
    } else {
        Write-Info "Skipping native build (use -SkipNativeBuild to always skip)"
    }
    
    # Restore NuGet packages
    Write-Header "Restoring NuGet packages"
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "Package restore failed"
    }
    Write-Success "NuGet packages restored"
    
    # Build .NET solution
    Write-Header "Building .NET solution"
    $buildArgs = @("build", "--configuration", $Configuration, "--no-restore")
    if ($Verbose) {
        $buildArgs += "--verbosity", "detailed"
    }
    
    dotnet @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw ".NET build failed"
    }
    Write-Success ".NET solution built successfully"
    
    # Download test fonts if they don't exist
    Write-Header "Checking test assets"
    $fontDir = "tests/ResvgSharp.Tests/TestAssets/fonts"
    $interRegular = "$fontDir/Inter-Regular.ttf"
    $interBold = "$fontDir/Inter-Bold.ttf"
    
    if (-not (Test-Path $interRegular) -or -not (Test-Path $interBold)) {
        Write-Info "Test fonts not found. Downloading Inter fonts..."
        
        $tempFile = [System.IO.Path]::GetTempFileName()
        $extractDir = [System.IO.Path]::GetTempPath() + "inter-fonts"
        
        try {
            # Download Inter font release
            $interUrl = "https://github.com/rsms/inter/releases/download/v4.0/Inter-4.0.zip"
            Write-Info "Downloading from $interUrl"
            Invoke-WebRequest -Uri $interUrl -OutFile $tempFile
            
            # Extract ZIP
            Write-Info "Extracting fonts..."
            Expand-Archive -Path $tempFile -DestinationPath $extractDir -Force
            
            # Copy required fonts
            Copy-Item "$extractDir/Inter-Regular.ttf" $fontDir -Force
            Copy-Item "$extractDir/Inter-Bold.ttf" $fontDir -Force
            
            Write-Success "Test fonts downloaded successfully"
        } catch {
            Write-Error "Failed to download test fonts: $_"
            Write-Info "Please manually download Inter-Regular.ttf and Inter-Bold.ttf to $fontDir"
        } finally {
            Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
            Remove-Item $extractDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    } else {
        Write-Success "Test fonts already present"
    }
    
    # Run tests
    Write-Header "Running tests"
    $testArgs = @("test", "--configuration", $Configuration, "--no-build")
    if ($Verbose) {
        $testArgs += "--verbosity", "detailed"
    } else {
        $testArgs += "--verbosity", "normal"
    }
    
    dotnet @testArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed"
    }
    
    Write-Header "Build and test completed successfully!"
    Write-Success "All tests passed"
    
} catch {
    Write-Error $_.Exception.Message
    exit 1
}