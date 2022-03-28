dotnet ef migrations add InitialCreate -s ..\TimePlanner.WebApi\TimePlanner.WebApi.csproj

dotnet ef database update -s ..\TimePlanner.WebApi\TimePlanner.WebApi.csproj

dotnet ef database update -s ..\TimePlanner.WebApi\TimePlanner.WebApi.csproj --connection Server=ALEKSEY-HPZB\SQLEXPRESS;Database=TimePlanner;Trusted_Connection=True;MultipleActiveResultSets=true
