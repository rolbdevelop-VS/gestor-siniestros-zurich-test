import type { ClaimStatus } from '../types'
import { formatStatus } from '../utils/formatters'

interface StatusBadgeProps {
  status: ClaimStatus
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const styles: Record<ClaimStatus, { bg: string; color: string; border: string }> = {
    Open: { bg: '#ebf8ff', color: '#2b6cb0', border: '#bee3f8' },
    InReview: { bg: '#fefcbf', color: '#975a16', border: '#fef08a' },
    Closed: { bg: '#f0fff4', color: '#22543d', border: '#c6f6d5' },
  }

  const current = styles[status] ?? { bg: '#edf2f7', color: '#4a5568', border: '#e2e8f0' }

  return (
    <span
      style={{
        display: 'inline-block',
        padding: '0.2rem 0.55rem',
        borderRadius: '9999px',
        fontSize: '0.8rem',
        fontWeight: 600,
        backgroundColor: current.bg,
        color: current.color,
        border: `1px solid ${current.border}`,
        whiteSpace: 'nowrap',
      }}
    >
      {formatStatus(status)}
    </span>
  )
}

