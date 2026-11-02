import type { ClaimStatus } from '../types'

export const CLAIM_STATUS_LABELS: Record<ClaimStatus, string> = {
  Open: 'Abierto',
  InReview: 'En revisión',
  Closed: 'Cerrado',
}

export function formatStatus(status: ClaimStatus): string {
  return CLAIM_STATUS_LABELS[status] ?? status
}

export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

export function formatDateTime(utcDateString: string): string {
  if (!utcDateString) return '—'
  const date = new Date(utcDateString)
  return new Intl.DateTimeFormat('es-MX', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}
