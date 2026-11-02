const USER_ID_REGEX = /^\d{1,20}$/

export function validateUserId(id: string): string | null {
  const trimmed = id.trim()
  if (!trimmed) {
    return 'El usuario es requerido.'
  }
  if (!USER_ID_REGEX.test(trimmed)) {
    return 'El usuario debe ser numérico y contener entre 1 y 20 dígitos.'
  }
  return null
}
