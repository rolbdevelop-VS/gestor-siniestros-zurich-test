# AI_LOG — Bitácora de Uso Crítico de Inteligencia Artificial

- **Proyecto:** Gestor de Siniestros Simplificado (Prueba Técnica Fullstack)
- **Candidato:** Brayan Robles · **Vacante:** ZCC | .NET Developer (Zurich México)
- **Herramienta:** Google Antigravity (Pair Programming con agente IA: Claude Sonnet 4.6 / 5.5 & Gemini 3.8 Flash)
- **Periodo:** 31 de octubre al 2 de noviembre de 2026 (31 turnos de interacción)
- **Evidencias en repo:** 31 commits incrementales, 73 pruebas automatizadas (`ZurichClaims.Api.Tests`), archivo de pruebas HTTP (`ZurichClaims.Api.http`) e historial auditado en [`Chat_Antigravity_History.md`](./Chat_Antigravity_History.md).

---

## 1. Filosofía de Trabajo y Control

La IA se utilizó como un acelerador para tareas repetitivas y generación de boilerplate (estructuras base, DTOs, métodos iniciales de EF Core y componentes UI). Sin embargo, la dirección técnica y la validación fueron 100% mías bajo tres reglas:
1. **Plan previo antes de codificar:** Ninguna línea de código se escribió sin un desglose paso a paso aprobado previamente.
2. **Reproducción antes de solución:** Cada defecto reportado debía reproducirse con evidencia en consola antes de autorizar un fix.
3. **Validación independiente:** No acepté reportes de "todo verificado" sin comprobarlo por mi cuenta con pruebas automatizadas, llamadas HTTP directas, inspección física en base de datos (DB Browser) y pruebas de UI en navegador.

---

## 2. Puntos Clave de Decisión e Intervención Crítica

### A. Alineación al Perfil y Stack Técnico (Turnos 1–2)
- **Propuesta de la IA:** Inicialmente la IA optó por Node.js/TypeScript y Express con un diseño genérico.
- **Mi intervención:** Modifiqué el rumbo a ASP.NET Core (.NET 8). Era imperativo demostrar dominio en el stack principal de la vacante (`.NET DEVELOPER`) y garantizar consistencia de tipos de punta a punta.

### B. Blindaje de Seguridad y Limpieza de Historial Git (Turnos 4–5)
- **Problema detectado:** El PDF del requerimiento técnico (marcado como `INTERNAL USE ONLY`) había sido versionado en los primeros commits.
- **Mi intervención:** Verifiqué que no existiera ningún repositorio remoto vinculado (`git remote -v` vacío). Detuve el flujo, respaldé el archivo fuera de la solución y reescribí el historial de Git con `git filter-branch` y purga de `reflog`, eliminando por completo cualquier rastro del documento antes de continuar.

### C. Tipado Estricto de Dominio vs. Strings Libres (Turnos 4–5)
- **Problema detectado:** La IA implementó los estados del siniestro como cadenas sueltas (`"en revisión"`, `"abierto"`), lo que abría la puerta a inconsistencias tipográficas y problemas de internacionalización.
- **Mi intervención:** Exigí el modelado mediante el enum fuertemente tipado `ClaimStatus` en C#, mapeado como texto en la base de datos vía EF Core y serializado como string estricto en la API, rechazando cadenas arbitrarias o identificadores numéricos ambiguos.

### D. Trazabilidad Real y Restricción de Integridad (Turnos 8–9)
- **Decisión de arquitectura propia:** El requerimiento original no exigía trazabilidad por usuario ni eliminación segura.
- **Mi intervención:** Introduje tres reglas de negocio obligatorias:
  1. Encabezado `x-usuario` requerido en todos los endpoints, auditado en la creación y en cada cambio de estado.
  2. Borrado puramente lógico (`IsDeleted`, `DeletedAt`, `DeletedBy`) con `HasQueryFilter` global.
  3. Relación de historial con `DeleteBehavior.Restrict` a nivel base de datos para impedir que borrados físicos accidentales dejen huérfana la auditoría.

### E. Detección de Falsos Positivos y Casos Borde en API (Turnos 13–18)
- **Problema detectado:** La IA dio por completado y probado el backend tras validar únicamente casos felices. Al someter los endpoints a una matriz manual de 20 casos borde encontré fallos graves:
  - `?policyNumber=%` actuaba como comodín SQL devolviendo todos los registros.
  - El monto máximo de C# (`decimal.MaxValue`) provocaba un error de servidor (HTTP 500 `OverflowException`) dentro del validador personalizado de decimales.
  - La migración dejaba el campo de auditoría vacío (`""`) en filas preexistentes.
- **Mi intervención:** Obligué a reproducir cada fallo con logs antes de corregir: escapado de comodines (`\`, `%`, `_`) con `EF.Functions.Like`, refactorización matemática con `Math.Round` para evitar desbordamientos, y valor por defecto `"0"` (sistema) en auditorías de migración.

### F. Control de Desbordamiento y Precisión en Frontend (Turnos 20–22)
- **Problema detectado:** Al probar la interfaz en el navegador, descubrí que montos grandes eran convertidos por JavaScript a `number`, provocando que `JSON.stringify` emitiera notación exponencial (`7.92e+28`) rechazada por la API, además de romper visualmente la tabla de siniestros.
- **Mi intervención:**
  - Establecí un tope de negocio coherente en ambos extremos ($0.01$ a $999,999,999,999.99).
  - Validé la entrada con expresiones regulares antes de cualquier parseo numérico en frontend.
  - Apliqué reglas de contención en CSS (`white-space: nowrap`, `overflow-wrap: anywhere`, `minWidth`) para impedir que datos largos deformaran la interfaz.

### G. Hallazgo de Cultura Regional en Pruebas Automatizadas (Turnos 24–26)
- **Problema detectado:** Una prueba con culturas (`es-ES`, `de-DE`) arrojó una excepción no controlada (`ArgumentException`). `[Range(typeof(decimal), "0.01", ...)]` parseaba sus límites con la cultura del hilo local, por lo que en entornos con coma decimal cualquier creación fallaba con HTTP 500.
- **Mi intervención:** Detecté además que la IA había relajado aserciones en el archivo de pruebas para disimular un error. Rechacé la prueba debilitada, investigué la causa raíz e instruí la configuración explícita de `ParseLimitsInInvariantCulture = true` y `ConvertValueInInvariantCulture = true` en producción antes de reincorporar la suite completa.

---

## 3. Conclusiones y Aprendizaje sobre Asistentes de IA

1. **Tendencia a la complacencia y al falso éxito:** Los modelos tienden a declarar tareas como "100% verificadas" tras validar únicamente el flujo feliz. La verificación independiente en el entorno de ejecución real sigue siendo responsabilidad insustituible del ingeniero.
2. **Fabricación de mérito:** Si se le pide al asistente que documente correcciones, suele presentar requisitos omitidos como "errores que corrigió por iniciativa propia". El criterio editorial de la documentación técnica debe mantenerse bajo control humano.
3. **Balance de productividad:** La IA aportó una aceleración significativa (generación de 73 pruebas xUnit, configuraciones de Vite y contratos base), permitiéndome enfocar el tiempo y el análisis en robustez de datos, integridad referencial, casos extremos y manejo seguro de errores.