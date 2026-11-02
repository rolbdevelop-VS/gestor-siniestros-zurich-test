import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { claimsApi } from '../api/claimsApi'
import { ErrorMessage } from '../components/ErrorMessage'
import { StatusBadge } from '../components/StatusBadge'
import { useUser } from '../context/useUser'
import type { ClaimStatus, ClaimSummaryResponse } from '../types'
import { formatCurrency, formatDateTime } from '../utils/formatters'

export function ClaimsListPage() {
  const { isValidUser } = useUser()

  const [claims, setClaims] = useState<ClaimSummaryResponse[]>([])
  const [loading, setLoading] = useState<boolean>(true)
  const [error, setError] = useState<unknown | null>(null)

  const [statusFilter, setStatusFilter] = useState<ClaimStatus | ''>('')
  const [policyInput, setPolicyInput] = useState<string>('')
  const [debouncedPolicy, setDebouncedPolicy] = useState<string>('')
  const [retryTrigger, setRetryTrigger] = useState<number>(0)

  // Debounce de 300 ms en la búsqueda por póliza
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedPolicy(policyInput)
    }, 300)
    return () => clearTimeout(timer)
  }, [policyInput])

  // Carga de datos con cancelación mediante AbortController para evitar race conditions
  useEffect(() => {
    if (!isValidUser) {
      return
    }

    const controller = new AbortController()
    let isMounted = true

    async function fetchClaims() {
      try {
        setLoading(true)
        setError(null)
        const data = await claimsApi.getAll(statusFilter || undefined, debouncedPolicy || undefined, controller.signal)
        if (isMounted) {
          setClaims(data)
          setLoading(false)
        }
      } catch (err: unknown) {
        if (err instanceof DOMException && err.name === 'AbortError') {
          return
        }
        if (isMounted) {
          setError(err)
          setLoading(false)
        }
      }
    }

    void fetchClaims()

    return () => {
      isMounted = false
      controller.abort()
    }
  }, [statusFilter, debouncedPolicy, isValidUser, retryTrigger])

  const handleClearFilters = () => {
    setStatusFilter('')
    setPolicyInput('')
    setDebouncedPolicy('')
  }

  const handleRetry = () => {
    setLoading(true)
    setError(null)
    setRetryTrigger((prev) => prev + 1)
  }

  return (
    <div>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          marginBottom: '1.5rem',
        }}
      >
        <div>
          <h1 style={{ fontSize: '1.75rem', color: '#002855', margin: '0 0 0.25rem 0' }}>
            Listado de Siniestros
          </h1>
          <p style={{ color: '#718096', margin: 0, fontSize: '0.95rem' }}>
            Consulta, filtra y gestiona los siniestros registrados.
          </p>
        </div>

        <Link
          to="/claims/new"
          style={{
            backgroundColor: '#0072ce',
            color: 'white',
            padding: '0.6rem 1.2rem',
            borderRadius: '6px',
            textDecoration: 'none',
            fontWeight: 600,
            fontSize: '0.95rem',
            display: 'inline-flex',
            alignItems: 'center',
            gap: '0.4rem',
            boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
          }}
        >
          <span>+</span>
          <span>Registrar Siniestro</span>
        </Link>
      </div>

      {/* Barra de Filtros */}
      <div
        style={{
          backgroundColor: 'white',
          padding: '1rem 1.25rem',
          borderRadius: '8px',
          boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
          marginBottom: '1.5rem',
          display: 'flex',
          flexWrap: 'wrap',
          gap: '1rem',
          alignItems: 'flex-end',
        }}
      >
        <div style={{ flex: '1 1 200px' }}>
          <label
            htmlFor="filterStatus"
            style={{ display: 'block', fontSize: '0.85rem', fontWeight: 600, color: '#4a5568', marginBottom: '0.35rem' }}
          >
            Filtrar por estado
          </label>
          <select
            id="filterStatus"
            value={statusFilter}
            onChange={(e) => {
              setLoading(true)
              setStatusFilter(e.target.value as ClaimStatus | '')
            }}
            style={{
              width: '100%',
              padding: '0.5rem 0.75rem',
              borderRadius: '6px',
              border: '1px solid #cbd5e0',
              fontSize: '0.9rem',
              backgroundColor: 'white',
            }}
          >
            <option value="">Todos los estados</option>
            <option value="Open">Abierto</option>
            <option value="InReview">En revisión</option>
            <option value="Closed">Cerrado</option>
          </select>
        </div>

        <div style={{ flex: '2 1 280px' }}>
          <label
            htmlFor="filterPolicy"
            style={{ display: 'block', fontSize: '0.85rem', fontWeight: 600, color: '#4a5568', marginBottom: '0.35rem' }}
          >
            Buscar por número de póliza
          </label>
          <input
            id="filterPolicy"
            type="text"
            value={policyInput}
            onChange={(e) => {
              setLoading(true)
              setPolicyInput(e.target.value)
            }}
            placeholder="Ej. POL-2024..."
            style={{
              width: '100%',
              padding: '0.5rem 0.75rem',
              borderRadius: '6px',
              border: '1px solid #cbd5e0',
              fontSize: '0.9rem',
            }}
          />
        </div>

        {(statusFilter || policyInput) && (
          <button
            type="button"
            onClick={handleClearFilters}
            style={{
              padding: '0.5rem 1rem',
              borderRadius: '6px',
              border: '1px solid #cbd5e0',
              backgroundColor: '#edf2f7',
              color: '#4a5568',
              fontSize: '0.9rem',
              cursor: 'pointer',
              fontWeight: 500,
            }}
          >
            Limpiar filtros
          </button>
        )}
      </div>

      {Boolean(error) && <ErrorMessage error={error} onRetry={handleRetry} />}

      {loading && (
        <div style={{ textAlign: 'center', padding: '3rem', color: '#718096' }}>
          <p style={{ fontSize: '1.1rem' }}>Cargando siniestros...</p>
        </div>
      )}

      {!loading && !error && claims.length === 0 && (
        <div
          style={{
            backgroundColor: 'white',
            padding: '3rem 1rem',
            borderRadius: '8px',
            textAlign: 'center',
            boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
          }}
        >
          <p style={{ fontSize: '1.1rem', color: '#718096', margin: '0 0 1rem 0' }}>
            No se encontraron siniestros con los criterios seleccionados.
          </p>
          {(statusFilter || policyInput) && (
            <button
              type="button"
              onClick={handleClearFilters}
              style={{
                backgroundColor: '#0072ce',
                color: 'white',
                border: 'none',
                padding: '0.5rem 1rem',
                borderRadius: '6px',
                cursor: 'pointer',
                fontSize: '0.9rem',
              }}
            >
              Restablecer filtros
            </button>
          )}
        </div>
      )}

      {!loading && !error && claims.length > 0 && (
        <div
          style={{
            backgroundColor: 'white',
            borderRadius: '8px',
            boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
            overflowX: 'auto',
          }}
        >
          <table style={{ width: '100%', minWidth: '950px', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.9rem' }}>
            <thead>
              <tr style={{ backgroundColor: '#f7fafc', borderBottom: '2px solid #edf2f7', color: '#4a5568' }}>
                <th style={{ padding: '0.85rem 1rem', fontWeight: 600, minWidth: '120px' }}>Póliza</th>
                <th style={{ padding: '0.85rem 1rem', fontWeight: 600, minWidth: '150px' }}>Asegurado</th>
                <th style={{ padding: '0.85rem 1rem', fontWeight: 600, minWidth: '140px' }}>Tipo de siniestro</th>
                <th style={{ padding: '0.85rem 1rem', fontWeight: 600, textAlign: 'right', whiteSpace: 'nowrap' }}>Monto estimado</th>
                <th style={{ padding: '0.85rem 1rem', fontWeight: 600, whiteSpace: 'nowrap' }}>Estado</th>
                <th style={{ padding: '0.85rem 1rem', fontWeight: 600, whiteSpace: 'nowrap' }}>Fecha de registro</th>
                <th style={{ padding: '0.85rem 1rem', fontWeight: 600, textAlign: 'center', whiteSpace: 'nowrap', minWidth: '130px' }}>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {claims.map((claim) => (
                <tr
                  key={claim.id}
                  style={{
                    borderBottom: '1px solid #edf2f7',
                    transition: 'background-color 0.15s ease',
                  }}
                >
                  <td style={{ padding: '0.85rem 1rem', fontWeight: 600, color: '#002855', overflowWrap: 'anywhere', wordBreak: 'break-word' }}>
                    {claim.policyNumber}
                  </td>
                  <td style={{ padding: '0.85rem 1rem', overflowWrap: 'anywhere', wordBreak: 'break-word' }}>{claim.insuredName}</td>
                  <td style={{ padding: '0.85rem 1rem', color: '#4a5568', overflowWrap: 'anywhere', wordBreak: 'break-word' }}>{claim.claimType}</td>
                  <td style={{ padding: '0.85rem 1rem', textAlign: 'right', fontWeight: 600, whiteSpace: 'nowrap' }}>
                    {formatCurrency(claim.estimatedAmount)}
                  </td>
                  <td style={{ padding: '0.85rem 1rem', whiteSpace: 'nowrap' }}>
                    <StatusBadge status={claim.currentStatus} />
                  </td>
                  <td style={{ padding: '0.85rem 1rem', color: '#718096', fontSize: '0.85rem', whiteSpace: 'nowrap' }}>
                    {formatDateTime(claim.createdAt)}
                  </td>
                  <td style={{ padding: '0.85rem 1rem', textAlign: 'center', whiteSpace: 'nowrap', minWidth: '130px' }}>
                    <Link
                      to={`/claims/${claim.id}`}
                      style={{
                        color: '#0072ce',
                        textDecoration: 'none',
                        fontWeight: 600,
                        fontSize: '0.85rem',
                        padding: '0.25rem 0.5rem',
                        borderRadius: '4px',
                        border: '1px solid #bee3f8',
                        whiteSpace: 'nowrap',
                        display: 'inline-block',
                      }}
                    >
                      Ver detalle →
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
