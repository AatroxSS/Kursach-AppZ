using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Hospital.UI.Providers;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient http)
    {
        _jsRuntime = jsRuntime;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous();
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }

    public void MarkUserAsAuthenticated(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));
    }

    private async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
        }
        catch
        {
            return null;
        }
    }

    private static AuthenticationState Anonymous()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt) || !jwt.Contains('.'))
        {
            return Array.Empty<Claim>();
        }

        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            if (values is null)
            {
                return Array.Empty<Claim>();
            }

            var claims = new List<Claim>();

            foreach (var (rawKey, value) in values)
            {
                var claimType = rawKey switch
                {
                    "name" or "unique_name" => ClaimTypes.Name,
                    "nameid" or "sub" => ClaimTypes.NameIdentifier,
                    "role" or "roles" => ClaimTypes.Role,
                    _ when rawKey.EndsWith("/nameidentifier", StringComparison.OrdinalIgnoreCase) => ClaimTypes.NameIdentifier,
                    _ when rawKey.EndsWith("/name", StringComparison.OrdinalIgnoreCase) => ClaimTypes.Name,
                    _ when rawKey.EndsWith("/role", StringComparison.OrdinalIgnoreCase) => ClaimTypes.Role,
                    _ => rawKey
                };

                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                    {
                        claims.Add(new Claim(claimType, item.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(claimType, value.ToString()));
                }
            }

            return claims;
        }
        catch
        {
            return Array.Empty<Claim>();
        }
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }
}
