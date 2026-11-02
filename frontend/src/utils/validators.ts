export const MIN_ESTIMATED_AMOUNT = 0.01
export const MAX_ESTIMATED_AMOUNT = 999999999999.99

export function validateAmount(value: string): { isValid: boolean; amount?: number; error?: string } {
  const trimmed = value.trim()
  if (!trimmed) {
    return { isValid: false, error: 'El monto estimado es obligatorio.' }
  }

  // Expresión estricta: dígitos enteros y opcionalmente punto con 1 o 2 decimales (rechaza signos y notación científica)
  const strictDecimalRegex = /^\d+(\.\d{1,2})?$/
  if (!strictDecimalRegex.test(trimmed)) {
    return {
      isValid: false,
      error: 'El monto estimado debe ser un número positivo válido con un máximo de 2 decimales (sin signos ni exponentes).',
    }
  }

  const parsed = Number(trimmed)
  if (Number.isNaN(parsed) || parsed < MIN_ESTIMATED_AMOUNT) {
    return { isValid: false, error: 'El monto estimado debe ser mayor a 0.' }
  }

  if (parsed > MAX_ESTIMATED_AMOUNT) {
    return {
      isValid: false,
      error: 'El monto estimado no puede exceder 999,999,999,999.99.',
    }
  }

  return { isValid: true, amount: parsed }
}
