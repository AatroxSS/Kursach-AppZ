# Kursach Blazor loader fix v10

Fixes the error introduced by the previous cache fix:
Uncaught (in promise) Error: Failed to start platform.
Reason: Error: For a dotnetjs resource, custom loaders must supply a URI string.

Cause:
In .NET 10, loadBootResource can return fetch(...) for many boot assets,
but dotnetjs / JS runtime modules must return null or a URI string.

This patch changes Hospital.UI/wwwroot/index.html:
- dotnetjs and js-module-* resources return a URI string;
- other resources use fetch(..., cache: "no-store") without SRI;
- cache-busting query is still applied.

Apply:
Expand-Archive -Path .\kursach_blazor_loader_fix_v10.zip -DestinationPath . -Force

Then:
powershell -ExecutionPolicy Bypass -File .\tools\fix-blazor-cache-v10.ps1
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"

Run API:
dotnet run --project .\Hospital.API\Hospital.API.csproj --launch-profile https

Run UI:
dotnet run --project .\Hospital.UI\Hospital.UI.csproj --launch-profile http

Browser cleanup:
- DevTools -> Application -> Storage -> Clear site data
- DevTools -> Application -> Service Workers -> Unregister if any
- Open http://localhost:5174/?fresh=v10
