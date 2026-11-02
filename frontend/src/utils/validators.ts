export function validateAmount(value: string): { isValid: boolean; amount?: number; error?: string } {
  const trimmed = value.trim()
  if (!trimmed) {
    return { isValid: false, error: 'El monto estimado es obligatorio.' }
  }
  if (/[eE+-]/.test(trimmed)) {
    return { isValid: false, error: 'El monto no debe contener signos (+/-) ni notación científica (e/E).' }
  }
  // Expresión regular: dígitos enteros y opcionalmente punto con 1 o 2 decimales
  const decimalRegex = /^\d+(\.\d{1,2})?$/
  if (!decimalRegex.test(trimmed)) {
    return { isValid: false, error: 'El monto estimado debe ser un número positivo con máximo 2 decimales.' }
  }
  const parsed = Number(trimmed)
  if (Number.isNaN(parsed) || parsed <= 0) {
    return { isValid: false, error: 'El monto estimado debe ser estrictamente mayor a 0.' }
  }
  return { isValid: true, amount: parsed }
}
