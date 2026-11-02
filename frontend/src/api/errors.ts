import type { ProblemDetails } from '../types'

export class ApiError extends Error {
  public readonly status: number
  public readonly title: string
  public readonly detail?: string
  public readonly type?: string
  public readonly errors?: Record<string, string[]>

  constructor(status: number, title: string, detail?: string, type?: string, errors?: Record<string, string[]>) {
    super(detail || title)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.detail = detail
    this.type = type
    this.errors = errors
  }

  public static isInvalidStatusTransition(error: unknown): boolean {
    return error instanceof ApiError && error.type === 'https://api.zurich.claims/errors/invalid-status-transition'
  }

  public static isNotFound(error: unknown): boolean {
    return error instanceof ApiError && error.status === 404
  }
}

export async function parseApiError(response: Response): Promise<ApiError> {
  const status = response.status
  let data: ProblemDetails | null = null

  try {
    const text = await response.text()
    if (text) {
      data = JSON.parse(text) as ProblemDetails
    }
  } catch {
    // Si no es JSON válido
  }

  if (data) {
    const title = data.title || getDefaultTitleForStatus(status)
    const detail = data.detail || undefined
    const type = data.type || undefined
    const errors = data.errors || undefined
    return new ApiError(status, title, detail, type, errors)
  }

  return new ApiError(status, getDefaultTitleForStatus(status), `Error HTTP ${status}`)
}

export function createNetworkError(originalError?: unknown): ApiError {
  const message = originalError instanceof Error ? originalError.message : 'Error desconocido'
  return new ApiError(0, 'Error de conexión', 'No se pudo conectar con la API. Verifica que el servidor esté en ejecución.', undefined, {
    network: [message],
  })
}

function getDefaultTitleForStatus(status: number): string {
  switch (status) {
    case 400:
      return 'Solicitud incorrecta'
    case 404:
      return 'Recurso no encontrado'
    case 500:
      return 'Error interno del servidor'
    default:
      return 'Error al procesar la solicitud'
  }
}
