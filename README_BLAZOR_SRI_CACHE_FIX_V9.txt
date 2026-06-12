# Kursach Blazor SRI/cache fix v9

Fixes browser errors like:
- Failed to find a valid digest in the integrity attribute
- SRI's integrity checks failed
- GET /_framework/Hospital.UI.<hash>.wasm 404
- GET /_framework/Hospital.UI.<hash>.pdb 404

Cause:
Browser / Blazor boot cache is using stale fingerprinted asset names after repeated clean/build/run.
The app asks for old .wasm/.pdb filenames that no longer exist in the current build.

Files:
- Hospital.UI/wwwroot/index.html
- tools/fix-blazor-cache.ps1

Apply:
Expand-Archive -Path .\kursach_blazor_sri_cache_fix_v9.zip -DestinationPath . -Force

Then run:
powershell -ExecutionPolicy Bypass -File .\tools\fix-blazor-cache.ps1
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"

Run:
dotnet run --project .\Hospital.API\Hospital.API.csproj --launch-profile https
dotnet run --project .\Hospital.UI\Hospital.UI.csproj --launch-profile http

Browser:
1. Open DevTools
2. Application -> Storage -> Clear site data
3. Application -> Service Workers -> Unregister if any exists
4. Network -> Disable cache
5. Open http://localhost:5174/?fresh=v9

For a quick test, Incognito mode should also work.
