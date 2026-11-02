import { Outlet } from 'react-router-dom'
import { Header } from './Header'
import { useUser } from '../context/useUser'

export function Layout() {
  const { isValidUser, validationError } = useUser()

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', backgroundColor: '#f8f9fa' }}>
      <Header />

      {!isValidUser && (
        <div
          style={{
            backgroundColor: '#fffaf0',
            borderBottom: '1px solid #feebc8',
            color: '#c05621',
            padding: '0.75rem 1.5rem',
            textAlign: 'center',
            fontSize: '0.9rem',
          }}
          role="alert"
        >
          <strong>⚠️ Atención:</strong> {validationError} Debes ingresar un número de usuario válido en la barra superior para interactuar con el sistema de siniestros.
        </div>
      )}

      <main style={{ flex: 1, maxWidth: '1200px', width: '100%', margin: '0 auto', padding: '1.5rem' }}>
        <Outlet />
      </main>

      <footer
        style={{
          borderTop: '1px solid #e2e8f0',
          padding: '1rem',
          textAlign: 'center',
          color: '#718096',
          fontSize: '0.85rem',
          backgroundColor: 'white',
        }}
      >
        Gestor de Siniestros Simplificado &copy; 2026 Zurich Insurance
      </footer>
    </div>
  )
}
