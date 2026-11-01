# AI_LOG - Registro de Uso Crítico y Decisiones de Arquitectura

**Proyecto:** Gestor de Siniestros Simplificado (Zurich Claims)  
**Candidato:** Bryan Robles  
**Posición:** ZCC | .NET DEVELOPER  
**Asistente de IA:** Antigravity (Google DeepMind)

---

## Hito 1: Configuración Inicial, Persistencia y Modelado de Datos

### 1. Objetivo del Hito
Establecer la estructura base del proyecto, configurar el entorno de desarrollo y diseñar la capa de persistencia relacional con Entity Framework Core 8 y SQLite, asegurando una arquitectura robusta, normalizada y versionada mediante migraciones.

---

### 2. Análisis Crítico y Correcciones a las Propuestas de la IA

| Decisión de Arquitectura | Propuesta Inicial de la IA | Corrección / Decisión Adoptada | Justificación Técnica y de Negocio |
| :--- | :--- | :--- | :--- |
| **Stack de Backend** | Node.js + Express + TypeScript | **ASP.NET Core Web API (.NET 8 LTS)** | La vacante técnica evaluada es específicamente **"ZCC \| .NET DEVELOPER"**. Aunque el documento general permitía Node.js, utilizar .NET 8 con C# demuestra directamente el dominio técnico y patrones empresariales en el stack objetivo de Zurich. |
| **Versiones de Paquetes EF Core** | Instalar paquetes en su versión más reciente (floating/latest). | **Fijar estrictamente versión `8.0.11`** (`Microsoft.EntityFrameworkCore.Sqlite` y `Design`). | Evita incompatibilidades con previews o versiones posteriores (.NET 9/10), garantizando reproducibilidad y estabilidad en el runtime .NET 8 LTS. |
| **Inicialización de la Base de Datos** | Uso de `DbContext.Database.EnsureCreated()`. | **Uso de `Database.Migrate()` y versionado de la carpeta `Migrations/` en Git.** | `EnsureCreated()` no utiliza la tabla de historial de migraciones (`__EFMigrationsHistory`), impidiendo la evolución del esquema en producción. `Migrate()` aplica migraciones de manera trazable y profesional. |
| **Estructura del Historial de Estados** | Array JSON embebido dentro de la entidad `Claim` (`history: json`). | **Tabla relacional normalizada `ClaimStatusHistories` (1:N)** con Foreign Key y borrado en cascada. | Descartar el antipatrón NoSQL en base SQL. Cumple la 1ª Forma Normal (1NF), permite consultas analíticas e índices eficientes sobre cambios de estado y prepara la base para transacciones atómicas. |
| **Índice en `PolicyNumber`** | Índice único (`IsUnique(true)`). | **Índice NO único (`IsUnique(false)`).** | En el dominio de seguros, una póliza vigente puede presentar múltiples siniestros a lo largo de su vigencia. Un índice único bloquearía registrar un segundo siniestro para la misma póliza. |
| **Campos de Historial (`PreviousStatus`, `ChangeReason`)** | `PreviousStatus` requerido y `ChangeReason` requerido. | **`PreviousStatus` nullable (`string?`) y `ChangeReason` opcional.** | Al registrar un siniestro nuevo, el estado previo no existe (`null -> abierto`). Obligar un estado previo forzaría cadenas ficticias ("N/A") que ensucian el modelo. |
| **Determinismo en Seed Data** | Uso de `DateTime.UtcNow` dinámico en `HasData()`. | **Fechas UTC fijas determinísticas (`DateTimeKind.Utc`) e historial coherente.** | `DateTime.UtcNow` dentro de `HasData` produce snapshots mutables en EF Core, provocando falsas migraciones continuas. Además, se garantizó que los eventos del historial reflejen exactamente el ciclo de vida del estado actual de cada siniestro (abierto: 1 evento; en revisión: 2 eventos; cerrado: 3 eventos). |
| **Seguridad de CORS** | `AllowAnyOrigin()` por facilidad en desarrollo. | **CORS estricto limitado a `http://localhost:5173`** (puerto de Vite). | Cumplimiento del principio de mínimo privilegio y prevención de cross-origin exploitation indebido. |
| **Gestión de Artefactos de Base de Datos** | Excluir únicamente `.db`. | **Exclusión explícita en `.gitignore` de `*.db`, `*.db-shm` y `*.db-wal`.** | Previene que bases de datos locales y los archivos de Write-Ahead Logging (WAL) de SQLite se suban accidentalmente al control de versiones. |

---

### 3. Estado de Entrega del Hito 1
- [x] Solución `ZurichClaims.slnx` y proyecto API en `backend/ZurichClaims.Api` bajo `.NET 8`.
- [x] Paquetes fijados en EF Core `8.0.11` y herramienta local `dotnet-ef` versionada en `dotnet-tools.json`.
- [x] Modelos de dominio `Claim` y `ClaimStatusHistory` con relaciones fluidas y Data Annotations.
- [x] Migración inicial generada y probada (`Migrations/20261031225451_InitialCreate.cs`).
- [x] Pipeline configurado con `Migrate()`, CORS restringido y Swagger UI.
- [x] Base de datos y logs excluidos en `.gitignore`.

