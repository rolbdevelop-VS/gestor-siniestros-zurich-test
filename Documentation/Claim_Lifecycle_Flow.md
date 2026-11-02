# Ciclo de Vida y Flujo de Auditoría de Siniestros (Claims Lifecycle)

Este documento detalla el ciclo de vida del siniestro, las transiciones de estado permitidas y cómo se registra y audita el **número de usuario logueado (`x-usuario`)** en cada una de las interacciones dentro del sistema.

---

## 1. Máquina de Estados Finita del Siniestro

El sistema implementa una máquina de estados controlada por [`StatusTransitions.cs`](file:///c:/Proyectos/Zurich/ZCC/PruebaTecnica/backend/ZurichClaims.Api/Services/StatusTransitions.cs), evitando saltos arbitrarios entre estados:

```mermaid
stateDiagram-v2
    direction LR

    [*] --> Open : Creación del Siniestro (POST)\nAuditado: CreatedBy = x-usuario\nHistorial: (null → Open)

    Open --> InReview : Enviar a Revisión (POST /status)\nAuditado: ChangedBy = x-usuario
    InReview --> Open : Reabrir Siniestro (POST /status)\nAuditado: ChangedBy = x-usuario
    
    InReview --> Closed : Cerrar Siniestro (POST /status)\nAuditado: ChangedBy = x-usuario
    Closed --> InReview : Reabrir para Revisión (POST /status)\nAuditado: ChangedBy = x-usuario

    state "Transición Inválida (HTTP 400)" as InvalidTransition {
        Open --> Closed : Salto directo no permitido
        Closed --> Open : Reapertura directa no permitida
        Open --> Open : Mismo estado no permitido
        InReview --> InReview : Mismo estado no permitido
        Closed --> Closed : Mismo estado no permitido
    }

    note right of InvalidTransition
        Cualquier intento de transición no autorizada
        retorna HTTP 400 Bad Request con ProblemDetails
        type = ".../invalid-status-transition"
    end note
```

---

## 2. Diagrama de Flujo Completo: Interacciones y Registro del Usuario Logueado

El siguiente diagrama ilustra el flujo de extremo a extremo, mostrando cómo el identificador numérico del usuario logueado (`x-usuario`) es capturado en el cliente y propagado hacia la base de datos para garantizar la trazabilidad de auditoría en todas las operaciones:

```mermaid
flowchart TD
    %% Inicio y Autenticación
    Start([Inicio / Usuario]) --> LoginScreen["Pantalla de Login (/login)\nIngreso de Usuario (número > 0) y Contraseña (>= 4 dígitos)"]
    LoginScreen --> SaveSession["Frontend (UserContext & localStorage)\nGuarda userId activo (ej. '1001')"]
    SaveSession --> Navigation["Navegación / Vistas de la Aplicación\n(Listado, Formulario de Creación, Detalle)"]

    %% Creación
    Navigation -->|Opción 'Nuevo Siniestro'| CreateClaim["1. REGISTRO DE SINIESTRO\n(POST /api/claims)"]
    subgraph AuditCreation ["Auditoría de Creación"]
        CreateClaim --> SendCreateReq["Envío HTTP con Header: x-usuario: 1001\nJSON: policyNumber, insuredName, claimType, estimatedAmount"]
        SendCreateReq --> ValidCreate{"¿x-usuario y datos válidos?"}
        ValidCreate -- No --> Err400Create["HTTP 400 Bad Request\n(ValidationProblemDetails)"]
        ValidCreate -- Sí --> PersistCreate["Base de Datos (Claims & ClaimStatusHistories):\n- Claims.CurrentStatus = 'Open'\n- Claims.CreatedBy = '1001'\n- Claims.CreatedAt = UtcNow\n- Claims.UpdatedAt = null\n- ClaimStatusHistories.Add({\n    PreviousStatus: null,\n    NewStatus: 'Open',\n    ChangedBy: '1001',\n    ChangeReason: 'Apertura y registro inicial...'\n  })"]
        PersistCreate --> Res201["HTTP 201 Created (Location: /api/claims/{id})"]
    end

    %% Consulta
    Navigation -->|Opción 'Listado' / 'Detalle'| ReadClaim["2. CONSULTA DE SINIESTROS\n(GET /api/claims o GET /api/claims/{id})"]
    subgraph AuditRead ["Auditoría de Consulta"]
        ReadClaim --> SendReadReq["Envío HTTP con Header: x-usuario: 1001"]
        SendReadReq --> FilterDeleted["EF Core aplica QueryFilter global (!c.IsDeleted)\nRecupera detalle con historial completo ordenado cronológicamente"]
        FilterDeleted --> Res200Read["HTTP 200 OK con ClaimDetailResponse\n(Muestra trazabilidad histórica: quién creó y quién modificó)"]
    end

    %% Actualización
    Navigation -->|Opción 'Editar datos'| UpdateClaim["3. ACTUALIZACIÓN DE DATOS\n(PUT /api/claims/{id})"]
    subgraph AuditUpdate ["Auditoría de Modificación"]
        UpdateClaim --> SendUpdateReq["Envío HTTP con Header: x-usuario: 1001\nJSON: policyNumber, insuredName, claimType, estimatedAmount"]
        SendUpdateReq --> ValidUpdate{"¿Siniestro existe y no borrado?"}
        ValidUpdate -- No --> Err404Update["HTTP 404 Not Found"]
        ValidUpdate -- Sí --> PersistUpdate["Base de Datos (Claims):\n- Actualiza datos descriptivos y monto\n- Claims.UpdatedBy = '1001'\n- Claims.UpdatedAt = UtcNow\n(Estado e historial permanecen intactos)"]
        PersistUpdate --> Res200Update["HTTP 200 OK"]
    end

    %% Cambio de Estado
    Navigation -->|Opción 'Cambiar Estado'| StatusClaim["4. TRANSICIÓN DE ESTADO\n(POST /api/claims/{id}/status)"]
    subgraph AuditStatus ["Auditoría de Transición"]
        StatusClaim --> SendStatusReq["Envío HTTP con Header: x-usuario: 1001\nJSON: newStatus, reason (opcional)"]
        SendStatusReq --> CheckValidTransition{"¿Transición permitida en StatusTransitions?"}
        CheckValidTransition -- No --> Err400Transition["HTTP 400 Bad Request\n(type: .../invalid-status-transition)"]
        CheckValidTransition -- Sí --> PersistStatus["Base de Datos (Transacción EF Core):\n1. Claims.CurrentStatus = newStatus\n2. Claims.UpdatedBy = '1001'\n3. Claims.UpdatedAt = UtcNow\n4. ClaimStatusHistories.Add({\n     PreviousStatus: statusAnterior,\n     NewStatus: newStatus,\n     ChangeReason: reason,\n     ChangedBy: '1001',\n     ChangedAt: Claims.UpdatedAt\n   })"]
        PersistStatus --> Res200Status["HTTP 200 OK con Historial Actualizado"]
    end

    %% Borrado Lógico
    Navigation -->|Opción 'Eliminar'| DeleteClaim["5. BORRADO LÓGICO\n(DELETE /api/claims/{id})"]
    subgraph AuditDelete ["Auditoría de Eliminación"]
        DeleteClaim --> SendDeleteReq["Envío HTTP con Header: x-usuario: 1001"]
        SendDeleteReq --> CheckDeleteExists{"¿Existe y no está borrado?"}
        CheckDeleteExists -- No --> Err404Delete["HTTP 404 Not Found"]
        CheckDeleteExists -- Sí --> PersistDelete["Base de Datos (Claims):\n- Claims.IsDeleted = true\n- Claims.DeletedBy = '1001'\n- Claims.DeletedAt = UtcNow\n(Historial intacto, protegido por FK Restrict)"]
        PersistDelete --> Res204["HTTP 204 NoContent"]
    end
```

---

## 3. Matriz de Auditoría por Operación

La siguiente tabla resume qué campos de auditoría se actualizan en la base de datos en cada interacción y cómo se vincula el usuario de la sesión:

| Operación | Endpoint HTTP | Header Requerido | Campos Auditados en `Claims` | Registros Auditados en `ClaimStatusHistories` |
|---|---|---|---|---|
| **Creación** | `POST /api/claims` | `x-usuario: {id}` | `CreatedBy = {id}`<br>`CreatedAt = UtcNow`<br>`UpdatedAt = null`<br>`UpdatedBy = null` | Inserta fila inicial:<br>`PreviousStatus = null`<br>`NewStatus = Open`<br>`ChangedBy = {id}`<br>`ChangedAt = CreatedAt` |
| **Edición de Datos** | `PUT /api/claims/{id}` | `x-usuario: {id}` | `UpdatedBy = {id}`<br>`UpdatedAt = UtcNow`<br>*(Datos modificados)* | *(No genera fila de historial; solo actualiza auditoría de entidad)* |
| **Cambio de Estado** | `POST /api/claims/{id}/status` | `x-usuario: {id}` | `CurrentStatus = {newStatus}`<br>`UpdatedBy = {id}`<br>`UpdatedAt = UtcNow` | Inserta nueva fila histórica:<br>`PreviousStatus = {prevStatus}`<br>`NewStatus = {newStatus}`<br>`ChangeReason = {motivo}`<br>`ChangedBy = {id}`<br>`ChangedAt = UpdatedAt` |
| **Borrado Lógico** | `DELETE /api/claims/{id}` | `x-usuario: {id}` | `IsDeleted = true`<br>`DeletedBy = {id}`<br>`DeletedAt = UtcNow` | *(El historial histórico se preserva intacto para fines legales y de auditoría)* |
| **Consultas** | `GET /api/claims`<br>`GET /api/claims/{id}` | `x-usuario: {id}` | *(Solo lectura; valida formato de usuario)* | *(Retorna trazabilidad histórica completa al cliente)* |

---

## 4. Garantías de Integridad y Trazabilidad

1. **Inmutabilidad del Historial:** La tabla `ClaimStatusHistories` es de solo inserción (*append-only*). Ninguna acción de la API modifica ni elimina registros de historial existentes.
2. **Protección contra Borrado Físico Cascada:** La clave foránea está configurada con `OnDelete(DeleteBehavior.Restrict)`. Si una instrucción SQL física intentara ejecutar `DELETE FROM Claims WHERE Id = X`, el motor de base de datos SQLite/SQL Server rechazará la operación por violación de restricción referencial si el siniestro posee registros de historial.
3. **Consistencia Temporal:** En los cambios de estado, la marca temporal `ChangedAt` del historial es exactamente idéntica a `UpdatedAt` del siniestro, garantizando sincronización exacta entre la entidad y su bitácora.

