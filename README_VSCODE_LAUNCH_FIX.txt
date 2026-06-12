# Kursach VS Code Blazor launch fix

This patch avoids VS Code trying to open the Blazor debug proxy URL directly.

How to use:
1. Unpack this archive into the repository root with replacement.
2. Start API:
   dotnet run --project .\Hospital.API\Hospital.API.csproj --launch-profile https
3. Start UI:
   dotnet run --project .\Hospital.UI\Hospital.UI.csproj --launch-profile http
4. Open:
   http://localhost:5174

If you need UI debugging:
- First start UI with the command above.
- In VS Code Run and Debug choose:
  Attach Blazor UI (Edge, after dotnet run)
