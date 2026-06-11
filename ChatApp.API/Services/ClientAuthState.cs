using ChatApp.Contracts;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace ChatApp.Services;

public sealed class ClientAuthState(IJSRuntime jsRuntime, IHttpClientFactory httpClientFactory, NavigationManager navigation)
{
    public string? Token { get; private set; }

    public UserProfile? User { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && User is not null;

    public async Task InitializeAsync()
    {
        Token ??= await jsRuntime.InvokeAsync<string?>("chatApp.getToken");
        if (!string.IsNullOrWhiteSpace(Token) && User is null)
        {
            await RefreshProfileAsync();
        }
    }

    public async Task SignInAsync(AuthResponse response)
    {
        Token = response.Token;
        User = response.User;
        await jsRuntime.InvokeVoidAsync("chatApp.setToken", response.Token);
    }

    public async Task SignOutAsync()
    {
        Token = null;
        User = null;
        await jsRuntime.InvokeVoidAsync("chatApp.clearToken");
    }

    private async Task RefreshProfileAsync()
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(navigation.BaseUri);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

        try
        {
            User = await client.GetFromJsonAsync<UserProfile>("auth/me");
        }
        catch (HttpRequestException)
        {
            await SignOutAsync();
        }
    }
}
