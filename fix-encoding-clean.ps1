# UTF-8 Encoding Fix Script
# Converts all source files to UTF-8 encoding

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "UTF-8 Encoding Conversion Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to convert files to UTF-8
function Convert-ToUTF8 {
    param (
        [string]$Filter,
 [string]$Description
    )
    
    Write-Host "Processing $Description files..." -ForegroundColor Yellow
    $files = Get-ChildItem -Path . -Filter $Filter -Recurse -File | Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' }
    $successCount = 0
    $failCount = 0
    
    foreach ($file in $files) {
    try {
      # Read content with default encoding
      $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Write as UTF-8 with BOM
         $utf8 = New-Object System.Text.UTF8Encoding $true
       [System.IO.File]::WriteAllText($file.FullName, $content, $utf8)
          
          Write-Host "  [OK] $($file.Name)" -ForegroundColor Green
          $successCount++
        }
        catch {
   Write-Host "  [FAIL] $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
          $failCount++
   }
    }
    
    return @{Success = $successCount; Failed = $failCount}
}

# Process .cs files
Write-Host "[1/4] Converting .cs files..." -ForegroundColor Cyan
$csResult = Convert-ToUTF8 -Filter "*.cs" -Description "C# source"

Write-Host ""

# Process .xaml files
Write-Host "[2/4] Converting .xaml files..." -ForegroundColor Cyan
$xamlResult = Convert-ToUTF8 -Filter "*.xaml" -Description "XAML"

Write-Host ""

# Process .md files
Write-Host "[3/4] Converting .md files..." -ForegroundColor Cyan
$mdResult = Convert-ToUTF8 -Filter "*.md" -Description "Markdown"

Write-Host ""

# Build project
Write-Host "[4/4] Building project..." -ForegroundColor Cyan
try {
    dotnet clean | Out-Null
    $buildOutput = dotnet build 2>&1
    
    if ($LASTEXITCODE -eq 0) {
 Write-Host "  [OK] Build successful" -ForegroundColor Green
    } else {
     Write-Host "  [FAIL] Build failed" -ForegroundColor Red
     Write-Host $buildOutput
    }
}
catch {
    Write-Host "  [FAIL] Build error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Conversion Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "C# files:   Success: $($csResult.Success), Failed: $($csResult.Failed)" -ForegroundColor White
Write-Host "XAML files: Success: $($xamlResult.Success), Failed: $($xamlResult.Failed)" -ForegroundColor White
Write-Host "MD files:   Success: $($mdResult.Success), Failed: $($mdResult.Failed)" -ForegroundColor White
Write-Host ""

$totalSuccess = $csResult.Success + $xamlResult.Success + $mdResult.Success
$totalFailed = $csResult.Failed + $xamlResult.Failed + $mdResult.Failed

if ($totalFailed -eq 0) {
    Write-Host "[SUCCESS] All files converted successfully!" -ForegroundColor Green
} else {
    Write-Host "[WARNING] $totalFailed file(s) failed to convert" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
