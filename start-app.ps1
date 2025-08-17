# Start FinalSay - Backend and Frontend
Write-Host "Starting FinalSay..." -ForegroundColor Green

# Start API in background
Write-Host "Starting .NET API server..." -ForegroundColor Yellow
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd FinalSay.Api; dotnet run" -WindowStyle Normal

# Wait a few seconds for API to start
Start-Sleep 3

# Start Frontend
Write-Host "Starting Vue.js frontend..." -ForegroundColor Yellow
Set-Location final-say-frontend
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "npm run dev" -WindowStyle Normal

Write-Host "Both services are starting!" -ForegroundColor Green
Write-Host "API will be available at: http://localhost:5244" -ForegroundColor Cyan
Write-Host "Frontend will be available at: http://localhost:3000" -ForegroundColor Cyan
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
