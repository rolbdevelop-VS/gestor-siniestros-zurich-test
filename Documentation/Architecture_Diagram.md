# Arquitectura del Sistema — Gestor de Siniestros (Zurich Claims)

Este documento describe la arquitectura técnica, la distribución en capas y los flujos de comunicación entre los componentes del sistema desarrollado para la prueba técnica de Zurich Insurance Company (ZCC).

---

## 1. Diagrama de Arquitectura Global (C4 - Contenedores y Capas)

```mermaid
flowchart TD
    subgraph ClientLayer ["Capa de Cliente (Navegador Web)"]
        Browser["Navegador Web (Usuario Final)"]
    end

    subgraph FrontendSPA ["Frontend (React 19 + TypeScript + Vite)"]
        UIComponents["Componentes UI & Páginas\n(LoginPage, ClaimsListPage, ClaimDetailPage, ClaimForm)"]
        UserContext["UserContext & useUser\n(Gestión de Sesión y localStorage)"]
        ApiClient["Cliente API (claimsApi.ts)\n(Inyección automática de header x-usuario)"]
        ErrorHandling["Manejo de Errores (errors.ts)\n(Normalización de ProblemDetails RFC 7807)"]
        
        UIComponents --> UserContext
        UIComponents --> ApiClient
        ApiClient --> ErrorHandling
    end

    subgraph BackendAPI ["Backend (ASP.NET Core 8 Web API)"]
        subgraph Pipeline ["Pipeline de Middleware HTTP"]
            CorsMiddleware["CORS Policy\n(Restringido a http://localhost:5173)"]
            ExceptionHandler["GlobalExceptionHandler\n(ProblemDetails / ValidationProblemDetails)"]
            RoutingMiddleware["Routing & Controller Endpoint Mapping"]
        end

        subgraph Presentation ["Capa de Presentación / Controladores"]
            ClaimsController["ClaimsController\n([FromHeader] x-usuario: ^\\d{1,20}$)\nValidaciones DataAnnotations en DTOs"]
            Converters["StrictJsonStringEnumConverter\n(Enum strict parsing & formatting)"]
        end

        subgraph Application ["Capa de Aplicación / Servicios de Dominio"]
            ClaimService["ClaimService (IClaimService)\n(Lógica de negocio, orquestación y auditoría)"]
            StatusTransitions["StatusTransitions\n(Máquina de estados finita del siniestro)"]
        end

        subgraph DataAccess ["Capa de Acceso a Datos (EF Core 8)"]
            ClaimsDbContext["ClaimsDbContext\n(QueryFilter global !c.IsDeleted\nValueConverters UTC DateTime\nFK Restrict en historial)"]
        end
    end

    subgraph Storage ["Capa de Persistencia"]
        SQLiteDB[("SQLite Database\n(claims.db / :memory: en tests)\nTablas: Claims, ClaimStatusHistories")]
    end

    %% Relaciones
    Browser --> UIComponents
    ApiClient -- "HTTPS / JSON\nHeader: x-usuario: {userId}" --> CorsMiddleware
    CorsMiddleware --> ExceptionHandler
    ExceptionHandler --> RoutingMiddleware
    RoutingMiddleware --> ClaimsController
    ClaimsController --> Converters
    ClaimsController --> ClaimService
    ClaimService --> StatusTransitions
    ClaimService --> ClaimsDbContext
    ClaimsDbContext --> SQLiteDB
```

---

## 2. Descripción de Componentes y Responsabilidades

### 2.1 Capa Frontend (Single Page Application)
- **Vite + React 19 + TypeScript:** Aplicación cliente de alto rendimiento sin librerías externas de UI (CSS nativo y estilos en línea).
- **UserContext (`useUser`):** Administra la identidad del usuario logueado en la sesión. Se almacena en `localStorage` y valida que el identificador sea numérico entero $> 0$.
- **Cliente API Centralizado (`claimsApi.ts`):** Punto único de salida de peticiones HTTP con `fetch`. Extrae el `userId` activo y lo inyecta como encabezado `x-usuario` en cada solicitud (`GET`, `POST`, `PUT`, `DELETE`).
- **Mapeo de Errores (`ApiError`):** Parsea respuestas `ProblemDetails` y `ValidationProblemDetails`, abstrayendo la interfaz de usuario de mensajes genéricos.

### 2.2 Capa Backend (ASP.NET Core Web API)
- **Controlador REST (`ClaimsController`):**
  - Expone los endpoints REST para el ciclo de vida del siniestro.
  - Exige y valida el encabezado `[FromHeader(Name = "x-usuario")]` con expresión regular `^\d{1,20}$`.
  - Valida contratos de entrada mediante DTOs fuertemente tipados (`CreateClaimRequest`, `UpdateClaimRequest`, `ChangeStatusRequest`).
- **Conversor Estricto de Enums (`StrictJsonStringEnumConverter`):**
  - Deserializa y serializa estados (`ClaimStatus`) exclusivamente en formato texto (`"Open"`, `"InReview"`, `"Closed"`).
  - Rechaza números literales (ej. `0`, `1`, `99`) o cadenas numéricas (`"1"`).
- **Servicio de Dominio (`ClaimService`):**
  - Ejecuta las reglas de negocio de creación, actualización, cambio de estado y borrado lógico.
  - Aplica ventanas de tiempo para auditoría (`CreatedAt`, `UpdatedAt`, `DeletedAt`).
  - Consulta la máquina de estados `StatusTransitions.IsValid(origen, destino)`.
  - Escapa caracteres comodín (`\`, `%`, `_`) en búsquedas `LIKE` sobre el número de póliza.
- **Contexto de Base de Datos (`ClaimsDbContext`):**
  - Filtro global de borrado lógico: `entity.HasQueryFilter(c => !c.IsDeleted)`.
  - Restricción de integridad referencial: `OnDelete(DeleteBehavior.Restrict)` para impedir el borrado físico de siniestros con historial.
  - `ValueConverter` bidireccional para garantizar que todas las fechas recuperadas de SQLite mantengan `DateTimeKind.Utc`.

---

## 3. Diagrama de Secuencia de una Petición Típica (Auditoría con `x-usuario`)

```mermaid
sequenceDiagram
    autonumber
    actor Usuario as Usuario Logueado
    participant UI as Frontend (React SPA)
    participant Client as claimsApi.ts
    participant Ctrl as ClaimsController
    participant Svc as ClaimService
    participant DB as ClaimsDbContext (SQLite)

    Usuario->>UI: Realiza acción (ej. Registrar siniestro)
    UI->>Client: create(data)
    Note over Client: Inyecta encabezado:<br/>x-usuario: 1001
    Client->>Ctrl: POST /api/claims (JSON Body + Header x-usuario)
    
    rect rgb(240, 248, 255)
        Note over Ctrl: Validación automática:<br/>1. Header x-usuario matches ^\d{1,20}$<br/>2. DTO DataAnnotations (Range, MaxDecimalPlaces)
    end
    
    Ctrl->>Svc: CreateAsync(request, userId: "1001")
    Note over Svc: 1. Instancia Claim (Status=Open, CreatedBy="1001")<br/>2. Agrega ClaimStatusHistory inicial (ChangedBy="1001")
    Svc->>DB: Add(claim) & SaveChangesAsync()
    DB-->>Svc: Confirmación de persistencia con Id generado
    Svc-->>Ctrl: ClaimDetailResponse (DTO)
    Ctrl-->>Client: 201 Created (Location: /api/claims/{id})
    Client-->>UI: Objeto tipado ClaimDetailResponse
    UI-->>Usuario: Muestra detalle del siniestro creado
```

