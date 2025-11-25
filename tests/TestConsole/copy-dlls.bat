@echo off
REM Script para copiar DLLs do NativeWrapper para a pasta de testes
REM Uso: copy-dlls.bat

echo ========================================
echo Copiando DLLs para TestConsole
echo ========================================
echo.

set SOURCE_DIR=..\..\src\BinaryKits.Zpl.NativeWrapper\bin\x86\Release\net472
set DEST_DIR=.

echo Origem: %SOURCE_DIR%
echo Destino: %DEST_DIR%
echo.

if not exist "%SOURCE_DIR%" (
    echo ERRO: Pasta de origem nao encontrada!
    echo Execute primeiro o build-x86.ps1 na pasta src
    echo.
    pause
    exit /b 1
)

echo Copiando arquivos...
xcopy /Y /E /I "%SOURCE_DIR%\*" "%DEST_DIR%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo DLLs copiadas com sucesso!
    echo ========================================
    echo.
    echo Arquivos principais copiados:
    echo - BinaryKits.Zpl.NativeWrapper.dll
    echo - BinaryKits.Zpl.Viewer.dll
    echo - BinaryKits.Zpl.Label.dll
    echo - SkiaSharp.dll
    echo - E todas as dependencias
    echo.
    echo Pastas de bibliotecas nativas:
    echo - x86\
    echo - x64\
    echo - arm64\
    echo.
) else (
    echo.
    echo ERRO ao copiar arquivos!
    echo.
)

pause
