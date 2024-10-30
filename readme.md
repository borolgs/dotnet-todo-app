# ASP.NET Core Example App

## Usage

```bash
# Development

# Server
docker compose up

dotnet tool install --global dotnet-ef
dotnet ef database update --project server

dotnet watch --project server

dotnet test

# Client
npm i -g pnpm
pnpm dev

# Client: generate client types
pnpm openapi

# Server: update deps
dotnet tool install --global dotnet-outdated-tool
dotnet outdated
dotnet outdated -pre Always -u
```

## API

- [/swagger](http://localhost:5000/swagger)
- [/metrics](http://localhost:5000/metrics)
