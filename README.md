# Kick.Client

Modern .NET packages for the Kick platform API, OAuth 2.1 PKCE flows, and RSA-verified webhooks.

## Packages

- `Kick.Client`
- `Kick.Client.AspNetCore`
- `Kick.Client.DependencyInjection`
- `Kick.Client.Generated`

## What this repo provides

- a typed Kiota-based API client for the Kick platform API
- OAuth 2.1 PKCE helpers for desktop and local-app flows
- webhook signature verification and normalized event models
- ASP.NET Core route mapping helpers
- an interactive Spectre.Console sample that walks through tunnel setup, auth, and event subscriptions

## Install

```bash
dotnet add package Kick.Client
dotnet add package Kick.Client.AspNetCore
```

Add the DI package when you want service registration helpers:

```bash
dotnet add package Kick.Client.DependencyInjection
```

## Quick start

```csharp
using Kick.Client.Authentication;

KickOAuthOptions options = new()
{
    ClientId = "your-client-id",
    RedirectUri = "http://127.0.0.1:5200/oauth/callback",
    Scopes = "user:read channel:read events:subscribe"
};

string codeVerifier = KickPkceFlowHelper.GenerateCodeVerifier();
string codeChallenge = KickPkceFlowHelper.DeriveCodeChallenge(codeVerifier);
string state = Guid.NewGuid().ToString("N");

string authorizationUrl = KickPkceFlowHelper.BuildAuthorizationUrl(
    "https://id.kick.com",
    options,
    codeChallenge,
    state);
```

## ASP.NET Core webhook example

```csharp
using Kick.Client.AspNetCore;
using Kick.Client.Webhooks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddKickClient();

var app = builder.Build();
app.MapKickWebhook(
    "/webhooks/kick",
    static (_, _) => Task.FromResult(new KickWebhookOptions()),
    static (evt, _, _) =>
    {
        Console.WriteLine(evt.EventType);
        return Task.CompletedTask;
    });

await app.RunAsync();
```

## Sample

Run the interactive sample to host a webhook endpoint, expose it through Azure Dev Tunnels, complete PKCE auth, and subscribe to Kick webhook events:

```bash
dotnet run --project samples/Kick.Client.Sample
```

## Development

```bash
dotnet test Kick.Client.slnx
```
