@echo off
cls

.paket\paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket\paket.exe install -v
if errorlevel 1 (
  exit /b %errorlevel%
)

./fake.cmd build -f build.fsx %*
