# Docker Setup Guide

## Quick Start

### 1. Without Mock Data (Recommended for development)
```bash
docker-compose up
```

This will:
- Start SQL Server
- Initialize the database with tables and stored procedures
- Start the API server (http://localhost:8080/swagger)
- Start the Frontend (http://localhost:5173)

**Execution time:** ~30 seconds

### 2. With Mock Data (For performance testing)
```bash
GENERATE_MOCK_DATA=true docker-compose up
```

Or create a `.env` file:
```bash
cp .env.example .env
# Edit .env and set: GENERATE_MOCK_DATA=true
docker-compose up
```

This will additionally:
- Generate **1,000 employees**
- Generate **10 million attendance records** (spanning 3 years)
- Generate **Roles, Department**

**Execution time:** 5-15 minutes (mock data generation only, rest is ~30 seconds)

---

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `DB_PASSWORD` | `D2tts@rikkeisoft` | SQL Server SA password |
| `GENERATE_MOCK_DATA` | `false` | Set to `true` to generate 10M attendance records |

## Container Details

| Container | Port | Purpose |
|-----------|------|---------|
| `sqlserver` | 1433 | SQL Server database |
| `sql-init` | N/A | Database initialization (runs once, then exits) |
| `hrm-api` | 8080 | .NET Core API |
| `hrm-frontend` | 5173 | Vue 3 Frontend (via Nginx) |

## Useful Commands

### Start services in background
```bash
docker-compose up -d
```

### View logs
```bash
docker-compose logs -f sql-init      # Watch database init
docker-compose logs -f hrm-api       # Watch API logs
docker-compose logs -f hrm-frontend  # Watch frontend logs
```

### Stop and clean up
```bash
docker-compose down          # Stop containers
docker-compose down -v       # Stop and remove volumes (delete database data)
```

### Rebuild images
```bash
docker-compose build --no-cache
docker-compose up
```

---

## Mock Data Details

**What gets generated:**

1. **1,000 Employees**
   - Unique codes: EMP00001 - EMP01000
   - Realistic names, emails, phone numbers
   - Department assignments
   - Manager relationships
   - Hire dates: 3-5 years ago

2. **10 Million Attendance Records**
   - Dates: Last 3 years
   - Check-in times: 8:00 AM ± 1 hour
   - Check-out times: 5:00 PM ± 1 hour
   - Work hours: 7.5 - 9.5 hours/day
   - Overtime: 0 - 2 hours/day
   - Status: Present (1), Absent (2), Late (3), EarlyLeave (4)

**Performance notes:**
- Total data size: ~2-3 GB
- Insert rate: ~650K-1M records/minute
- Uses batching (500K records per batch) for stability
- SQL Server performance mode recommended for faster insertion

---

## Database Restore

If you need to reset the database:

```bash
# Stop and remove volumes
docker-compose down -v

# Start fresh (with mock data if desired)
GENERATE_MOCK_DATA=true docker-compose up
```

---

## Troubleshooting

### "SQL Server connection refused"
- Ensure `sqlserver` container is running: `docker ps`
- Wait 30+ seconds for SQL Server to fully initialize
- Check logs: `docker-compose logs sqlserver`

### "Mock data generation taking too long"
- This is normal (5-15 minutes)
- Monitor progress in logs: `docker-compose logs sql-init`
- Ensure sufficient disk space (3+ GB)

### "Out of memory or disk space"
- Stop containers: `docker-compose down`
- Free up disk space
- Restart: `docker-compose up`

### "Port already in use"
Edit `docker-compose.yml` and change port mappings:
```yaml
services:
  sqlserver:
    ports:
      - "1434:1433"  # Changed from 1433
  hrm-api:
    ports:
      - "8081:8080"  # Changed from 8080
```

---

## Performance Tips

- **Mock data generation:** Close unnecessary applications to speed up
- **SQL queries:** Add indexes after mock data generation if needed
- **API:** Implement pagination for large datasets (already done in codebase)

