# Kursach loading fix v5

Fixes the browser stuck forever on the Blazor "Loading" screen.

Cause:
Hospital.UI/wwwroot/index.html used:
<script src="_framework/blazor.webassembly#[.{fingerprint}].js"></script>

If the fingerprint placeholder is not replaced during local run/build, the browser can't load the Blazor runtime script and the app remains on the Loading screen.

This patch replaces it with the stable development script:
<script src="_framework/blazor.webassembly.js"></script>

Apply:
Expand-Archive -Path .\kursach_loading_fix_v5.zip -DestinationPath . -Force

Then:
dotnet clean "Cursach AppZ.slnx"
Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"

Run API:
dotnet run --project .\Hospital.API\Hospital.API.csproj --launch-profile https

Run UI in another terminal:
dotnet run --project .\Hospital.UI\Hospital.UI.csproj --launch-profile http

Open:
http://localhost:5174

Important:
Press Ctrl+F5 in the browser or clear site data for localhost:5174 after applying.
