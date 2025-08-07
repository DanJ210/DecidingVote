@echo off
echo Starting Voting App...
echo.

echo Starting .NET API server...
start "API Server" /D "VotingApp.Api" dotnet run

echo Waiting for API to start...
timeout /t 3 /nobreak > nul

echo Starting Vue.js frontend...
start "Frontend" /D "voting-app-frontend" npm run dev

echo.
echo Both services are starting!
echo API will be available at: http://localhost:5244
echo Frontend will be available at: http://localhost:3000
echo.
pause
