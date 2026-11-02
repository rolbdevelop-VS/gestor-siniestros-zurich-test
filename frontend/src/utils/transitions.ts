import type { ClaimStatus } from '../types'

// Reglas de transición en el cliente para una experiencia de usuario guiada.
// NOTA: El backend es la única fuente de la verdad para validar transiciones;
// si el backend responde 400 (ProblemDetails), se muestra directamente su mensaje.
export const ALLOWED_TRANSITIONS: Record<ClaimStatus, ClaimStatus[]> = {
  Open: ['InReview'],
  InReview: ['Open', 'Closed'],
  Closed: ['InReview'],
}

