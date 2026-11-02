import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useUser } from '../context/useUser'

export function LoginPage() {
  const navigate = useNavigate()
  const { login } = useUser()

  const [userInput, setUserInput] = useState<string>('')
  const [passwordInput, setPasswordInput] = useState<string>('')
  const [errors, setErrors] = useState<{ user?: string; password?: string }>({})

  const validate = (): boolean => {
    const newErrors: { user?: string; password?: string } = {}
    const trimmedUser = userInput.trim()

    // Validación: Usuario > 0 y numérico de 1 a 20 dígitos
    if (!trimmedUser) {
      newErrors.user = 'El campo usuario es obligatorio.'
    } else if (!/^\d{1,20}$/.test(trimmedUser) || BigInt(trimmedUser) <= 0n) {
      newErrors.user = 'El usuario debe ser un número entero mayor a 0 (máx. 20 dígitos).'
    }

    // Validación: Contraseña >= 4 dígitos
    if (!passwordInput) {
      newErrors.password = 'La contraseña es obligatoria.'
    } else if (passwordInput.length < 4) {
      newErrors.password = 'La contraseña debe tener al menos 4 dígitos.'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    if (!validate()) return

    login(userInput.trim())
    navigate('/')
  }

  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: '#002855',
        padding: '1.5rem',
      }}
    >
      <div
        style={{
          backgroundColor: 'white',
          borderRadius: '12px',
          padding: '2.5rem',
          maxWidth: '420px',
          width: '100%',
          boxShadow: '0 8px 24px rgba(0,0,0,0.25)',
        }}
      >
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>🛡️</div>
          <h1 style={{ fontSize: '1.5rem', color: '#002855', margin: '0 0 0.5rem 0' }}>
            Zurich Claims
          </h1>
          <p style={{ color: '#718096', margin: 0, fontSize: '0.9rem' }}>
            Acceso al Gestor de Siniestros
          </p>
        </div>

        <form onSubmit={handleSubmit} noValidate style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div>
            <label
              htmlFor="username"
              style={{ display: 'block', fontSize: '0.85rem', fontWeight: 600, color: '#2d3748', marginBottom: '0.35rem' }}
            >
              Usuario
            </label>
            <input
              id="username"
              type="text"
              inputMode="numeric"
              maxLength={20}
              value={userInput}
              onChange={(e) => setUserInput(e.target.value)}
              placeholder="Número del colaborador"
              style={{
                width: '100%',
                padding: '0.65rem 0.8rem',
                borderRadius: '6px',
                border: `1px solid ${errors.user ? '#e53e3e' : '#cbd5e0'}`,
                fontSize: '0.95rem',
              }}
            />
            {errors.user && (
              <p style={{ margin: '0.3rem 0 0', fontSize: '0.8rem', color: '#e53e3e' }}>
                {errors.user}
              </p>
            )}
          </div>

          <div>
            <label
              htmlFor="password"
              style={{ display: 'block', fontSize: '0.85rem', fontWeight: 600, color: '#2d3748', marginBottom: '0.35rem' }}
            >
              Contraseña
            </label>
            <input
              id="password"
              type="password"
              value={passwordInput}
              onChange={(e) => setPasswordInput(e.target.value)}
              placeholder="Clave de acceso"
              style={{
                width: '100%',
                padding: '0.65rem 0.8rem',
                borderRadius: '6px',
                border: `1px solid ${errors.password ? '#e53e3e' : '#cbd5e0'}`,
                fontSize: '0.95rem',
              }}
            />
            {/* <small style={{ display: 'block', marginTop: '0.25rem', color: '#a0aec0', fontSize: '0.75rem' }}>
              Contraseña maestra
            </small> */}
            {errors.password && (
              <p style={{ margin: '0.3rem 0 0', fontSize: '0.8rem', color: '#e53e3e' }}>
                {errors.password}
              </p>
            )}
          </div>

          <button
            type="submit"
            style={{
              marginTop: '0.5rem',
              padding: '0.75rem',
              borderRadius: '6px',
              border: 'none',
              backgroundColor: '#0072ce',
              color: 'white',
              fontSize: '1rem',
              fontWeight: 600,
              cursor: 'pointer',
              boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
            }}
          >
            Iniciar Sesión
          </button>
        </form>
      </div>
    </div>
  )
}

