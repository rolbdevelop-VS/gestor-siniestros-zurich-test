import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div style={{ textAlign: 'center', padding: '3rem 1rem' }}>
      <h1 style={{ fontSize: '3rem', color: '#002855', margin: '0 0 1rem' }}>404</h1>
      <h2 style={{ fontSize: '1.5rem', margin: '0 0 1rem', color: '#2d3748' }}>
        Página no encontrada
      </h2>
      <p style={{ color: '#718096', marginBottom: '2rem' }}>
        La ruta solicitada no existe o ha sido movida.
      </p>
      <Link
        to="/"
        style={{
          display: 'inline-block',
          backgroundColor: '#0072ce',
          color: 'white',
          padding: '0.6rem 1.2rem',
          borderRadius: '4px',
          textDecoration: 'none',
          fontWeight: 500,
        }}
      >
        Volver al listado de siniestros
      </Link>
    </div>
  )
}

