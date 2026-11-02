import { ApiError } from '../api/errors'

interface ErrorMessageProps {
  error: unknown
  onRetry?: () => void
  onDismiss?: () => void
}

export function ErrorMessage({ error, onRetry, onDismiss }: ErrorMessageProps) {
  if (!error) return null

  let title = 'Ha ocurrido un error'
  let detail: string | undefined
  let fieldErrors: Record<string, string[]> | undefined

  if (error instanceof ApiError) {
    title = error.title
    detail = error.detail
    fieldErrors = error.errors
  } else if (error instanceof Error) {
    detail = error.message
  } else if (typeof error === 'string') {
    detail = error
  }

  return (
    <div
      style={{
        backgroundColor: '#fff5f5',
        border: '1px solid #feb2b2',
        borderRadius: '6px',
        padding: '1rem',
        margin: '1rem 0',
        color: '#9b2c2c',
      }}
      role="alert"
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <strong style={{ fontSize: '1rem', display: 'block', marginBottom: '0.25rem' }}>
            {title}
          </strong>
          {detail && <p style={{ margin: '0 0 0.5rem 0', fontSize: '0.9rem' }}>{detail}</p>}
        </div>
        {onDismiss && (
          <button
            type="button"
            onClick={onDismiss}
            style={{
              background: 'none',
              border: 'none',
              fontSize: '1.25rem',
              cursor: 'pointer',
              color: '#9b2c2c',
              padding: '0 0.5rem',
            }}
            aria-label="Cerrar"
          >
            ×
          </button>
        )}
      </div>

      {fieldErrors && Object.keys(fieldErrors).length > 0 && (
        <ul style={{ margin: '0.5rem 0 0', paddingLeft: '1.25rem', fontSize: '0.85rem' }}>
          {Object.entries(fieldErrors).map(([field, messages]) => (
            <li key={field}>
              <strong>{field}:</strong> {messages.join(', ')}
            </li>
          ))}
        </ul>
      )}

      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          style={{
            marginTop: '0.75rem',
            backgroundColor: '#e53e3e',
            color: 'white',
            border: 'none',
            padding: '0.4rem 0.8rem',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '0.85rem',
          }}
        >
          Reintentar
        </button>
      )}
    </div>
  )
}
