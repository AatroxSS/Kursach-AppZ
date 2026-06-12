# Kursach SQLite schema fix v6

Fixes runtime registration error:
SQLite Error 1: 'no such column: u.FirstName'

Cause:
The code/model was updated and User now has FirstName/LastName, but the existing SQLite database file was created before these columns existed.

This patch updates Hospital.API/Program.cs:
- keeps API startup normal;
- calls EnsureCreated() for missing DB;
- checks PRAGMA table_info("Users");
- adds Users.FirstName and Users.LastName with default empty string if missing.

Apply:
Expand-Archive -Path .\kursach_sqlite_schema_fix_v6.zip -DestinationPath . -Force

Then:
dotnet clean "Cursach AppZ.slnx"
Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"

Run API:
dotnet run --project .\Hospital.API\Hospital.API.csproj --launch-profile https

Run UI:
dotnet run --project .\Hospital.UI\Hospital.UI.csproj --launch-profile http

If you do not need old data, an even simpler option is to delete the existing .db file and restart API.
