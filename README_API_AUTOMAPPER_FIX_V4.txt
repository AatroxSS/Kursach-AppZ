# Kursach API AutoMapper fix v4

Fixes:
Hospital.API/Program.cs(104,32): CS1503
Argument 2: cannot convert from 'System.Reflection.Assembly'
to 'System.Action<AutoMapper.IMapperConfigurationExpression>'

Cause:
AutoMapper 15+ changed AddAutoMapper overloads. The configuration action must be the first explicit argument.

Changed:
builder.Services.AddAutoMapper(typeof(HospitalMapperProfile).Assembly);

To:
builder.Services.AddAutoMapper(cfg => { }, typeof(HospitalMapperProfile));

Apply:
Expand-Archive -Path .\kursach_api_automapper_fix_v4.zip -DestinationPath . -Force

Then:
dotnet clean "Cursach AppZ.slnx"
Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"

Run:
dotnet run --project .\Hospital.API\Hospital.API.csproj --launch-profile https
dotnet run --project .\Hospital.UI\Hospital.UI.csproj --launch-profile http
