-- CREATE USER dev_man WITH PASSWORD 'dev'
-- GRANT ALL PRIVILEGES ON DATABASE "time_planner_development" to dev_man;
dotnet ef migrations add InitialCreate -s ..\TimePlanner.WebApi\TimePlanner.WebApi.csproj
dotnet ef database update -s ..\TimePlanner.WebApi\TimePlanner.WebApi.csproj
dotnet ef database update -s ..\TimePlanner.WebApi\TimePlanner.WebApi.csproj --connection Host=localhost;Port=5432;Database=time_planner;Username=time_planner_sa;Password=tpsa
or
Add-Migration InitialCreate
Update-Database
--
Add-Migration WorkItemNextTime
Script-Migration InitialCreate
