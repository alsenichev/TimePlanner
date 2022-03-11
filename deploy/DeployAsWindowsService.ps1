Get-Service -Name TimePlanner

Stop-Service -Name TimePlanner

Remove-Service -Name TimePlanner

dotnet publish c:\Projects\TimePlanner\TimePlanner.WebApi\TimePlanner.WebApi.csproj --output c:\Projects\TimePlanner\publish

New-Service -Name TimePlanner -BinaryPathName "c:\Projects\TimePlanner\publish\TimePlanner.WebApi.exe" -Credential "NT AUTHORITY\LocalService" -Description "Time and tasks tracking API" -DisplayName "TimePlanner" -StartupType Automatic

Start-Service -Name TimePlanner

Get-Service  -Name TimePlanner
