import { Link } from 'react-router-dom'
import { useUser } from '../context/useUser'

export function Header() {
  const { userId, setUserId, isValidUser, validationError } = useUser()

  return (
    <header
      style={{
        backgroundColor: '#002855',
        color: 'white',
        padding: '0.75rem 1.5rem',
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
      }}
    >
      <div
        style={{
          maxWidth: '1200px',
          margin: '0 auto',
          display: 'flex',
          flexWrap: 'wrap',
          justifyContent: 'space-between',
          alignItems: 'center',
          gap: '1rem',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem' }}>
          <Link
            to="/"
            style={{
              color: 'white',
              textDecoration: 'none',
              fontWeight: 'bold',
              fontSize: '1.25rem',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
            }}
          >
            <span>🛡️</span>
            <span>Zurich Claims</span>
          </Link>
          <nav style={{ display: 'flex', gap: '1rem' }}>
            <Link
              to="/"
              style={{
                color: '#e2e8f0',
                textDecoration: 'none',
                fontSize: '0.95rem',
                padding: '0.25rem 0.5rem',
                borderRadius: '4px',
              }}
            >
              Siniestros
            </Link>
            <Link
              to="/claims/new"
              style={{
                backgroundColor: '#0072ce',
                color: 'white',
                textDecoration: 'none',
                fontSize: '0.95rem',
                padding: '0.35rem 0.8rem',
                borderRadius: '4px',
                fontWeight: 500,
              }}
            >
              + Nuevo Siniestro
            </Link>
          </nav>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <label
            htmlFor="userIdInput"
            style={{ fontSize: '0.9rem', color: '#cbd5e1', fontWeight: 500 }}
          >
            Usuario (número):
          </label>
          <div style={{ position: 'relative' }}>
            <input
              id="userIdInput"
              type="text"
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
              placeholder="Ej. 1001"
              maxLength={20}
              style={{
                padding: '0.35rem 0.6rem',
                borderRadius: '4px',
                border: `2px solid ${isValidUser ? '#48bb78' : '#f56565'}`,
                backgroundColor: 'white',
                color: '#1a202c',
                fontSize: '0.9rem',
                width: '120px',
                fontWeight: 600,
              }}
              title={validationError ?? 'Usuario válido'}
            />
          </div>
          {!isValidUser && (
            <span
              style={{
                fontSize: '0.75rem',
                color: '#feb2b2',
                maxWidth: '180px',
                lineHeight: 1.2,
              }}
            >
              {validationError}
            </span>
          )}
        </div>
      </div>
    </header>
  )
}
