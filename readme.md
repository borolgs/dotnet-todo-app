# ASP.NET Core Example App

## Usage

```bash
# Development

# Server
docker compose up

dotnet tool install --global dotnet-ef
dotnet ef database update --project server

dotnet watch --project server

# Client
pnpm dev

# Client: generate client types
npx openapi-typescript http://localhost:5000/swagger/v1/swagger.json -o client/shared/api/schema.d.ts
```

## API

- [/swagger](http://localhost:5000/swagger)
- [/metrics](http://localhost:5000/metrics)
