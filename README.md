# ZgjedhjetApi - Assignment 2

Extension of Assignment 1 with Elasticsearch and Redis integration.

## Dependencies

- .NET 8.0
- SQL Server (Docker)
- Elasticsearch 8.11.0 (Docker)
- Redis (Docker)
- NEST 7.17.5
- StackExchange.Redis 2.8.16

### Why Docker?

Running SQL Server, Elasticsearch, and Redis in Docker containers was chosen for practical reasons on Fedora Linux:

- **SQL Server on Linux**: Microsoft's SQL Server doesn't have native Fedora packages, but runs perfectly in Docker
- **Elasticsearch compatibility**: Easier to manage specific versions and configurations without system-wide installation
- **Redis simplicity**: Quick to spin up without package conflicts
- **Clean development environment**: All services isolated in containers, easy to start/stop/remove without affecting the system
- **Consistent setup**: Same Docker commands work across any Linux distro

Running all three services was as simple as:
```bash
docker run -d --name sqlserver -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
docker run -d --name elasticsearch -p 9200:9200 -e "discovery.type=single-node" -e "xpack.security.enabled=false" elasticsearch:8.11.0
docker run -d --name redis -p 6379:6379 redis:latest
```

This approach avoided dealing with native package managers and kept the development environment portable.

## Setup

1. Start Docker containers:
```bash
docker start sqlserver elasticsearch redis
```

2. Run the API:
```bash
cd ZgjedhjetApi
dotnet run
```

API runs on: `http://localhost:5022`

## What I Built

### New Controller: ZgjedhjetElasticSearchController

4 endpoints:

1. **POST /api/ZgjedhjetElasticSearch/migrate**
   - Migrates data from SQL to Elasticsearch
   - Indexed 2951 records

2. **GET /api/ZgjedhjetElasticSearch**
   - Reads and filters data from Elasticsearch
   - Same logic as Assignment 1
   - Supports filtering by kategoria, komuna, qendra_e_votimit, vendvotimi, partia

3. **GET /api/ZgjedhjetElasticSearch/suggest**
   - Municipality autocomplete
   - Case-insensitive and accent-insensitive
   - Prefix matching

4. **GET /api/ZgjedhjetElasticSearch/statistics**
   - Shows most suggested municipalities
   - Data stored in Redis sorted set

## Testing & Results

### Test 1: Data Migration
```bash
curl -X POST http://localhost:5022/api/ZgjedhjetElasticSearch/migrate
```
Result: 2951 records migrated to Elasticsearch

### Test 2: Query Data
```bash
curl -X GET http://localhost:5022/api/ZgjedhjetElasticSearch
```
Result:
```json
{"results":[{"partia":"Partia111","totalVota":4688},{"partia":"Partia138","totalVota":899},...]}
```

### Test 3: Municipality Suggestions
```bash
curl -X GET "http://localhost:5022/api/ZgjedhjetElasticSearch/suggest?query=pri&top=5"
```
Result: `["Prishtinë","Prizren"]`
```bash
curl -X GET "http://localhost:5022/api/ZgjedhjetElasticSearch/suggest?query=decan&top=5"
```
Result: `["Deçan"]` (accent-insensitive working)

### Test 4: Statistics
```bash
curl -X GET "http://localhost:5022/api/ZgjedhjetElasticSearch/statistics?top=10"
```
Result:
```json
[{"komuna":"Prizren","nrISugjerimeve":2},{"komuna":"Prishtinë","nrISugjerimeve":1},{"komuna":"Deçan","nrISugjerimeve":1}]
```

## How It Works

- **Elasticsearch**: Uses custom analyzer with lowercase and asciifolding filters for accent-insensitive search
- **Redis**: Stores suggestion counts in a sorted set for efficient top-N queries
- **Filtering**: Same logic as Assignment 1, but reads from Elasticsearch instead of SQL
- **Autocomplete**: Prefix matching with match_phrase_prefix query

All requirements from the assignment PDF have been implemented and tested.
