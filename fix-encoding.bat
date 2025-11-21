@echo off
chcp 65001 >nul
echo ========================================
echo UTF-8 ??????
echo ========================================
echo.

echo [1/3] ???? .cs ??...
powershell -Command "$files = Get-ChildItem -Path . -Filter '*.cs' -Recurse; foreach ($file in $files) { try { $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::Default); $utf8 = New-Object System.Text.UTF8Encoding $true; [System.IO.File]::WriteAllText($file.FullName, $content, $utf8); Write-Host ('  ? ' + $file.Name) -ForegroundColor Green; } catch { Write-Host ('  ? ' + $file.Name + ' - ' + $_.Exception.Message) -ForegroundColor Red; } }"

echo.
echo [2/3] ???? .xaml ??...
powershell -Command "$files = Get-ChildItem -Path . -Filter '*.xaml' -Recurse; foreach ($file in $files) { try { $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::Default); $utf8 = New-Object System.Text.UTF8Encoding $true; [System.IO.File]::WriteAllText($file.FullName, $content, $utf8); Write-Host ('  ? ' + $file.Name) -ForegroundColor Green; } catch { Write-Host ('  ? ' + $file.Name + ' - ' + $_.Exception.Message) -ForegroundColor Red; } }"

echo.
echo [3/3] ???????...
dotnet clean >nul 2>&1
dotnet build

echo.
echo ========================================
echo ?????
echo ========================================
echo.
echo ????????????
echo ?????????????? ENCODING_FIX_GUIDE.md
echo.
pause
