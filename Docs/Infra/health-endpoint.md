# Health endpoint

The demo app now exposes a simple REST health endpoint at `/api/health`.

## What it does
- Returns HTTP 200 with a JSON payload when the app is healthy.
- Returns HTTP 503 with a simulated failure message when the `IS_BROKEN` setting is enabled in ASP.NET configuration (including environment-variable-based configuration).

## Example calls

### Healthy response
```bash
curl https://localhost:5001/api/health
```

Example JSON:
```json
{
  "status": "ok",
  "environment": "Development",
  "timestamp": "2026-06-05T00:00:00+00:00"
}
```

### Broken response (demo failure mode)
```bash
IS_BROKEN=true dotnet run --project TodoApp/TodoApp.csproj
curl https://localhost:5001/api/health
```

Expected result: HTTP 503 with a simulated service failure message.

## Notes
- This is intentionally simple for demo use.
- The fake error path is controlled entirely by the `IS_BROKEN` configuration value, which can come from environment variables in normal app hosting.
