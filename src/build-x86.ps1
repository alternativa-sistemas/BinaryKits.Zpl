# Build script to ensure all projects are compiled for x86 (32-bit)

Write-Host "Building BinaryKits.Zpl for x86 (32-bit)..." -ForegroundColor Cyan

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Yellow
dotnet clean -c Release

# Build Label project for x86
Write-Host "`nBuilding BinaryKits.Zpl.Label for x86..." -ForegroundColor Yellow
dotnet build "BinaryKits.Zpl.Label\BinaryKits.Zpl.Label.csproj" -c Release /p:Platform=x86 /p:PlatformTarget=x86

# Build Viewer project for x86
Write-Host "`nBuilding BinaryKits.Zpl.Viewer for x86..." -ForegroundColor Yellow
dotnet build "BinaryKits.Zpl.Viewer\BinaryKits.Zpl.Viewer.csproj" -c Release /p:Platform=x86 /p:PlatformTarget=x86

# Build NativeWrapper project for x86
Write-Host "`nBuilding BinaryKits.Zpl.NativeWrapper for x86..." -ForegroundColor Yellow
dotnet build "BinaryKits.Zpl.NativeWrapper\BinaryKits.Zpl.NativeWrapper.csproj" -c Release /p:Platform=x86 /p:PlatformTarget=x86

Write-Host "`nBuild complete!" -ForegroundColor Green
Write-Host "Output location: BinaryKits.Zpl.NativeWrapper\bin\x86\Release\net472\" -ForegroundColor Green
