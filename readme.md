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
npm i -g pnpm
pnpm dev

# Client: generate client types
pnpm openapi
```

## API

- [/swagger](http://localhost:5000/swagger)
- [/metrics](http://localhost:5000/metrics)
