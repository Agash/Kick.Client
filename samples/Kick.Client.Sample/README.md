# Kick.Client.Sample

Interactive console sample for:

- `Kick.Client`
- `Kick.Client.AspNetCore`
- `Kick.Client.DependencyInjection`
- `DevTunnels.Client`

This sample:

1. hosts a local ASP.NET Core webhook endpoint,
2. opens a Kick OAuth 2.1 PKCE authorization flow,
3. exposes the endpoint publicly with Azure Dev Tunnels,
4. subscribes to Kick webhook events,
5. prints normalized events live in the console.

## Run

```bash
dotnet run --project samples/Kick.Client.Sample
```
