@echo off
echo Starting FinalSay...
echo.

echo Starting .NET API server...
start "API Server" /D "FinalSay.Api" dotnet run

echo Waiting for API to start...
timeout /t 3 /nobreak > nul

echo Starting Vue.js frontend...
start "Frontend" /D "final-say-frontend" npm run dev

echo.
echo Both services are starting!
echo API will be available at: http://localhost:5244
echo Frontend will be available at: http://localhost:3000
echo.
pause
