$env:Path += ";C:\Users\medam\.dotnet"
Write-Host "Starting Real Estate Platform..." -ForegroundColor Cyan
dotnet run --project "WebApplication4\WebApplication4.csproj" --launch-profile "http"
