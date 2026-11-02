import { Link, useNavigate } from 'react-router-dom'
import { useUser } from '../context/useUser'

export function Header() {
  const { userId, isAuthenticated, logout } = useUser()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

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

        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <label
            htmlFor="userIdInput"
            style={{ fontSize: '0.9rem', color: '#cbd5e1', fontWeight: 500 }}
          >
            Usuario:
          </label>
          <div>
            <input
              id="userIdInput"
              type="text"
              readOnly
              value={userId}
              placeholder="Sin usuario"
              style={{
                padding: '0.35rem 0.6rem',
                borderRadius: '4px',
                border: '1px solid #cbd5e0',
                backgroundColor: '#f8fafc',
                color: '#1a202c',
                fontSize: '0.9rem',
                width: '110px',
                fontWeight: 600,
                cursor: 'default',
              }}
              title="Usuario de sesión activa (solo lectura)"
            />
          </div>

          {isAuthenticated ? (
            <button
              type="button"
              onClick={handleLogout}
              style={{
                backgroundColor: 'transparent',
                color: '#cbd5e1',
                border: '1px solid #4a5568',
                padding: '0.3rem 0.6rem',
                borderRadius: '4px',
                cursor: 'pointer',
                fontSize: '0.85rem',
              }}
            >
              Salir
            </button>
          ) : (
            <Link
              to="/login"
              style={{
                color: '#cbd5e1',
                textDecoration: 'none',
                fontSize: '0.85rem',
                border: '1px solid #4a5568',
                padding: '0.3rem 0.6rem',
                borderRadius: '4px',
              }}
            >
              Ingresar
            </Link>
          )}
        </div>
      </div>
    </header>
  )
}
