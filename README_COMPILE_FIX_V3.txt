# Kursach compile fix v3

Fixes current build errors:
1. Hospital.UI/Pages/Index.razor no longer imports Hospital.BLL.DTO.
   UI should not require Hospital.BLL after removing the UI -> BLL project reference.
2. Hospital.DAL/Entities/User.cs now contains FirstName and LastName, because the current AuthService uses these properties.

Apply:
Expand-Archive -Path .\kursach_compile_fix_v3.zip -DestinationPath . -Force

Then run:
dotnet clean "Cursach AppZ.slnx"
Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"

If using SQLite with old database/migrations and a runtime DB error appears after compilation, delete the local .db file or create a migration for the added User.FirstName/User.LastName columns.
