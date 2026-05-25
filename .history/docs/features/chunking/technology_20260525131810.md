# Chunking - Technology Stack

## Core Technologies

### Language & Framework
- **C# 12** (.NET 8)
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core 8.0** - ORM

### Database
- **SQLite 3** - File-based database
- **Microsoft.EntityFrameworkCore.Sqlite 8.0**

### PDF Processing
- **Docnet.Core 2.6.0** - PDF text extraction
  - Cross-platform (Windows, Linux, macOS)
  - Uses Chromium's PDFium rendering engine
  - MIT license

## Why These Choices

### C# / .NET 8
**Rationale:**
- Strong typing reduces runtime errors
- Excellent async/await for I/O-bound operations
- Cross-platform (runs on Windows, Linux, macOS)
- Rich ecosystem of libraries
- First-class Entity Framework integration

**Alternatives Considered:**
- Python: Slower, weaker typing, better ML libraries
- TypeScript: Good for web, limited for system tasks
- Go: Fast, but less mature ecosystem

### SQLite
**Rationale:**
- No server setup required
- File-based: Easy backup and portability
- ACID transactions
- Good for <1GB databases
- Excellent .NET support

**Alternatives Considered:**
- PostgreSQL: Overkill for single-user system
- SQL Server: Windows-only, heavier
- MongoDB: Schema flexibility not needed

### Docnet.Core
**Rationale:**
- Actively maintained (last update: 2024)
- Fast rendering (native C++ PDFium)
- Clean API: `GetDocReader() → GetPageReader() → GetText()`
- Cross-platform
- No external dependencies

**Alternatives Considered:**
- **iTextSharp**: Commercial license required
- **PdfPig (UglyToad)**: Version conflicts with .NET 8
- **PDFsharp**: Limited text extraction capabilities

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
<PackageReference Include="Docnet.Core" Version="2.6.0" />
```

### Runtime Requirements
- .NET 8 Runtime
- Windows 10+ / Linux / macOS
- ~50MB disk space for PDF binaries

## Future Technology Upgrades

### Planned
1. **Semantic Chunking**
   - Add: `Microsoft.ML.Tokenizers`
   - Why: Better boundary detection than regex

2. **Search Integration**
   - Add: `Lucene.Net` or `SQLite FTS5`
   - Why: Full-text search within chunks

3. **Parallel Processing**
   - Use: `System.Threading.Channels`
   - Why: Process multiple sources concurrently

### Under Consideration
- **ML.NET** - Adaptive chunking based on content type
- **SentenceTransformers.NET** - Embedding-based boundaries
- **RabbitMQ/Redis** - Job queue for background processing

---

**See also:**
- [README.md](./README.md) - Feature overview
- [modules.md](./modules.md) - Code structure
