# UTF-8 ??????
# ???????????????

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "UTF-8 ??????" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ????
function Convert-ToUTF8 {
    param (
        [string]$Filter
    )
    
    $files = Get-ChildItem -Path . -Filter $Filter -Recurse -File
    $successCount = 0
    $failCount = 0
    
    foreach ($file in $files) {
   try {
          # ????????????
$content = Get-Content $file.FullName -Raw -Encoding Default
       
            # ?? UTF-8 with BOM
   $utf8 = New-Object System.Text.UTF8Encoding $true
       [System.IO.File]::WriteAllText($file.FullName, $content, $utf8)
            
            Write-Host "  ? $($file.Name)" -ForegroundColor Green
   $successCount++
        }
        catch {
       Write-Host "  ? $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
            $failCount++
        }
    }
    
    return @{Success = $successCount; Failed = $failCount}
}

# ?? .cs ??
Write-Host "[1/3] ???? .cs ??..." -ForegroundColor Yellow
$csResult = Convert-ToUTF8 -Filter "*.cs"

Write-Host ""

# ?? .xaml ??
Write-Host "[2/3] ???? .xaml ??..." -ForegroundColor Yellow
$xamlResult = Convert-ToUTF8 -Filter "*.xaml"

Write-Host ""

# ???????
Write-Host "[3/3] ???????..." -ForegroundColor Yellow
try {
    dotnet clean | Out-Null
    $buildResult = dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? ????" -ForegroundColor Green
    } else {
    Write-Host "  ? ????" -ForegroundColor Red
        Write-Host $buildResult
 }
}
catch {
    Write-Host "  ? ????: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "????" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "C# ??:   ?? $($csResult.Success) ???? $($csResult.Failed) ?" -ForegroundColor White
Write-Host "XAML ??: ?? $($xamlResult.Success) ???? $($xamlResult.Failed) ?" -ForegroundColor White
Write-Host ""

$totalSuccess = $csResult.Success + $xamlResult.Success
$totalFailed = $csResult.Failed + $xamlResult.Failed

if ($totalFailed -eq 0) {
    Write-Host "? ?????????" -ForegroundColor Green
} else {
    Write-Host "? ? $totalFailed ???????" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "????????????" -ForegroundColor Cyan
Write-Host "?????????????? ENCODING_FIX_GUIDE.md" -ForegroundColor Cyan
Write-Host ""
Write-Host "??????..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
