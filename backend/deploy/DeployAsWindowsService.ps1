Get-Service -Name TimePlanner

Stop-Service -Name TimePlanner

Remove-Service -Name TimePlanner

dotnet publish c:\Projects\TimePlanner\backend\TimePlanner.WebApi\TimePlanner.WebApi.csproj --output c:\Projects\TimePlanner\backend\publish

New-Service -Name TimePlanner -BinaryPathName "c:\Projects\TimePlanner\backend\publish\TimePlanner.WebApi.exe" -Credential "NT AUTHORITY\LocalService" -Description "Time and tasks tracking API" -DisplayName "TimePlanner" -StartupType Automatic

Start-Service -Name TimePlanner

Get-Service  -Name TimePlanner
