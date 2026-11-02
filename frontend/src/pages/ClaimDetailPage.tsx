import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { claimsApi } from '../api/claimsApi'
import { ApiError } from '../api/errors'
import { ErrorMessage } from '../components/ErrorMessage'
import { StatusBadge } from '../components/StatusBadge'
import { useUser } from '../context/useUser'
import type { ClaimDetailResponse, ClaimStatus } from '../types'
import { formatCurrency, formatDateTime, formatStatus } from '../utils/formatters'
import { ALLOWED_TRANSITIONS } from '../utils/transitions'

export function ClaimDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { isValidUser } = useUser()

  const claimId = id ? parseInt(id, 10) : undefined

  const [claim, setClaim] = useState<ClaimDetailResponse | null>(null)
  const [loading, setLoading] = useState<boolean>(false)
  const [error, setError] = useState<unknown | null>(null)

  // Estado para el panel de cambio de estado
  const [selectedNewStatus, setSelectedNewStatus] = useState<ClaimStatus | ''>('')
  const [statusReason, setStatusReason] = useState<string>('')
  const [submittingStatus, setSubmittingStatus] = useState<boolean>(false)
  const [statusError, setStatusError] = useState<unknown | null>(null)

  // Estado para confirmación de eliminación
  const [showDeleteConfirm, setShowDeleteConfirm] = useState<boolean>(false)
  const [deleting, setDeleting] = useState<boolean>(false)
  const [deleteError, setDeleteError] = useState<unknown | null>(null)

  useEffect(() => {
    if (!claimId || !isValidUser) {
      return
    }

    const controller = new AbortController()
    let isMounted = true

    async function fetchDetail() {
      try {
        setLoading(true)
        setError(null)
        const data = await claimsApi.getById(claimId!, controller.signal)
        if (isMounted) {
          setClaim(data)
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

    void fetchDetail()

    return () => {
      isMounted = false
      controller.abort()
    }
  }, [claimId, isValidUser])

  // Obtener las transiciones permitidas según el estado actual
  const allowedNextStatuses = claim ? ALLOWED_TRANSITIONS[claim.currentStatus] || [] : []

  const handleChangeStatus = async (e: FormEvent) => {
    e.preventDefault()
    if (!claim || !selectedNewStatus || submittingStatus || !isValidUser) return

    setSubmittingStatus(true)
    setStatusError(null)

    try {
      const updated = await claimsApi.changeStatus(claim.id, {
        newStatus: selectedNewStatus,
        reason: statusReason.trim() ? statusReason.trim() : null,
      })
      // Refresca el detalle en pantalla con la respuesta del backend
      setClaim(updated)
      setSelectedNewStatus('')
      setStatusReason('')
    } catch (err: unknown) {
      setStatusError(err)
    } finally {
      setSubmittingStatus(false)
    }
  }

  const handleDeleteClaim = async () => {
    if (!claim || deleting || !isValidUser) return

    setDeleting(true)
    setDeleteError(null)

    try {
      await claimsApi.delete(claim.id)
      navigate('/')
    } catch (err: unknown) {
      setDeleteError(err)
      setDeleting(false)
      setShowDeleteConfirm(false)
    }
  }

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '3rem', color: '#718096' }}>
        <p style={{ fontSize: '1.1rem' }}>Cargando información del siniestro...</p>
      </div>
    )
  }

  if (error) {
    const isNotFound = ApiError.isNotFound(error)
    return (
      <div style={{ maxWidth: '800px', margin: '0 auto', textAlign: 'center', padding: '2rem 1rem' }}>
        <div
          style={{
            backgroundColor: 'white',
            padding: '2.5rem',
            borderRadius: '8px',
            boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
          }}
        >
          <h2 style={{ color: '#002855', margin: '0 0 1rem' }}>
            {isNotFound ? 'Siniestro no encontrado' : 'Error al consultar siniestro'}
          </h2>
          <p style={{ color: '#718096', marginBottom: '1.5rem' }}>
            {isNotFound
              ? `El siniestro #${claimId} no existe o ha sido eliminado del sistema.`
              : 'Ocurrió un problema al obtener los datos.'}
          </p>
          <Link
            to="/"
            style={{
              display: 'inline-block',
              backgroundColor: '#0072ce',
              color: 'white',
              padding: '0.6rem 1.2rem',
              borderRadius: '6px',
              textDecoration: 'none',
              fontWeight: 500,
            }}
          >
            ← Volver al listado
          </Link>
        </div>
      </div>
    )
  }

  if (!claim) return null

  return (
    <div style={{ maxWidth: '1000px', margin: '0 auto' }}>
      {/* Encabezado y Navegación */}
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'flex-start',
          flexWrap: 'wrap',
          gap: '1rem',
          marginBottom: '1.5rem',
        }}
      >
        <div>
          <Link to="/" style={{ color: '#0072ce', textDecoration: 'none', fontSize: '0.9rem', fontWeight: 500 }}>
            ← Volver al listado
          </Link>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginTop: '0.35rem' }}>
            <h1 style={{ fontSize: '1.85rem', color: '#002855', margin: 0 }}>
              Siniestro #{claim.id}
            </h1>
            <StatusBadge status={claim.currentStatus} />
          </div>
          <p style={{ color: '#718096', margin: '0.25rem 0 0', fontSize: '0.95rem' }}>
            Póliza: <strong>{claim.policyNumber}</strong> &bull; Registrado el {formatDateTime(claim.createdAt)} por usuario <strong>{claim.createdBy}</strong>
          </p>
        </div>

        {/* Acciones Editar y Eliminar */}
        <div style={{ display: 'flex', gap: '0.75rem' }}>
          <Link
            to={`/claims/${claim.id}/edit`}
            style={{
              padding: '0.55rem 1.1rem',
              borderRadius: '6px',
              border: '1px solid #cbd5e0',
              backgroundColor: 'white',
              color: '#2d3748',
              textDecoration: 'none',
              fontSize: '0.9rem',
              fontWeight: 600,
            }}
          >
            ✎ Editar datos
          </Link>

          <button
            type="button"
            onClick={() => setShowDeleteConfirm(true)}
            disabled={!isValidUser}
            style={{
              padding: '0.55rem 1.1rem',
              borderRadius: '6px',
              border: '1px solid #feb2b2',
              backgroundColor: '#fff5f5',
              color: '#e53e3e',
              fontSize: '0.9rem',
              fontWeight: 600,
              cursor: isValidUser ? 'pointer' : 'not-allowed',
            }}
          >
            🗑 Eliminar
          </button>
        </div>
      </div>

      {Boolean(deleteError) && <ErrorMessage error={deleteError} onDismiss={() => setDeleteError(null)} />}

      {/* Modal de Confirmación de Borrado */}
      {showDeleteConfirm && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.5)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1000,
            padding: '1rem',
          }}
        >
          <div
            style={{
              backgroundColor: 'white',
              borderRadius: '8px',
              padding: '2rem',
              maxWidth: '450px',
              width: '100%',
              boxShadow: '0 4px 6px rgba(0,0,0,0.1)',
            }}
          >
            <h3 style={{ margin: '0 0 0.75rem', color: '#9b2c2c', fontSize: '1.25rem' }}>
              Confirmar Eliminación
            </h3>
            <p style={{ color: '#4a5568', margin: '0 0 1.5rem', fontSize: '0.95rem', lineHeight: 1.5 }}>
              ¿Estás seguro de que deseas eliminar el siniestro con póliza <strong>{claim.policyNumber}</strong>?
              Esta acción aplicará un borrado lógico y el registro ya no estará disponible.
            </p>
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem' }}>
              <button
                type="button"
                onClick={() => setShowDeleteConfirm(false)}
                disabled={deleting}
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
                Cancelar
              </button>
              <button
                type="button"
                onClick={handleDeleteClaim}
                disabled={deleting}
                style={{
                  padding: '0.5rem 1rem',
                  borderRadius: '6px',
                  border: 'none',
                  backgroundColor: '#e53e3e',
                  color: 'white',
                  fontSize: '0.9rem',
                  cursor: deleting ? 'not-allowed' : 'pointer',
                  fontWeight: 600,
                }}
              >
                {deleting ? 'Eliminando...' : 'Sí, eliminar'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Grid de Información y Panel de Cambio de Estado */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1.5rem', marginBottom: '1.5rem' }}>
        {/* Tarjeta de Información General */}
        <div
          style={{
            backgroundColor: 'white',
            borderRadius: '8px',
            padding: '1.5rem',
            boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
          }}
        >
          <h2 style={{ fontSize: '1.15rem', color: '#002855', margin: '0 0 1.25rem 0', borderBottom: '1px solid #edf2f7', paddingBottom: '0.75rem' }}>
            Información del Siniestro
          </h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem', fontSize: '0.95rem' }}>
            <div>
              <span style={{ color: '#718096', fontSize: '0.85rem', display: 'block' }}>Nombre del Asegurado</span>
              <strong style={{ color: '#2d3748' }}>{claim.insuredName}</strong>
            </div>
            <div>
              <span style={{ color: '#718096', fontSize: '0.85rem', display: 'block' }}>Tipo de Siniestro</span>
              <span style={{ color: '#2d3748' }}>{claim.claimType}</span>
            </div>
            <div>
              <span style={{ color: '#718096', fontSize: '0.85rem', display: 'block' }}>Monto Estimado</span>
              <strong style={{ color: '#002855', fontSize: '1.2rem' }}>{formatCurrency(claim.estimatedAmount)}</strong>
            </div>
            {claim.updatedAt && (
              <div>
                <span style={{ color: '#718096', fontSize: '0.85rem', display: 'block' }}>Última Actualización</span>
                <span style={{ color: '#4a5568' }}>
                  {formatDateTime(claim.updatedAt)} {claim.updatedBy ? `por usuario ${claim.updatedBy}` : ''}
                </span>
              </div>
            )}
          </div>
        </div>

        {/* Panel de Cambio de Estado */}
        <div
          style={{
            backgroundColor: 'white',
            borderRadius: '8px',
            padding: '1.5rem',
            boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          <h2 style={{ fontSize: '1.15rem', color: '#002855', margin: '0 0 0.5rem 0' }}>
            Cambiar Estado
          </h2>
          <p style={{ color: '#718096', margin: '0 0 1rem 0', fontSize: '0.85rem' }}>
            Solo se muestran las transiciones permitidas desde <strong>{formatStatus(claim.currentStatus)}</strong>.
          </p>

          {Boolean(statusError) && <ErrorMessage error={statusError} onDismiss={() => setStatusError(null)} />}

          {allowedNextStatuses.length === 0 ? (
            <p style={{ color: '#718096', fontStyle: 'italic', margin: 'auto 0' }}>
              No hay transiciones disponibles para este estado.
            </p>
          ) : (
            <form onSubmit={handleChangeStatus} style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: 'auto' }}>
              <div>
                <label
                  htmlFor="newStatusSelect"
                  style={{ display: 'block', fontSize: '0.85rem', fontWeight: 600, color: '#4a5568', marginBottom: '0.35rem' }}
                >
                  Nuevo estado <span style={{ color: '#e53e3e' }}>*</span>
                </label>
                <select
                  id="newStatusSelect"
                  value={selectedNewStatus}
                  onChange={(e) => setSelectedNewStatus(e.target.value as ClaimStatus)}
                  disabled={submittingStatus || !isValidUser}
                  style={{
                    width: '100%',
                    padding: '0.55rem 0.75rem',
                    borderRadius: '6px',
                    border: '1px solid #cbd5e0',
                    fontSize: '0.9rem',
                    backgroundColor: 'white',
                  }}
                  required
                >
                  <option value="">Selecciona el nuevo estado...</option>
                  {allowedNextStatuses.map((st) => (
                    <option key={st} value={st}>
                      {formatStatus(st)}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label
                  htmlFor="statusReasonInput"
                  style={{ display: 'block', fontSize: '0.85rem', fontWeight: 600, color: '#4a5568', marginBottom: '0.35rem' }}
                >
                  Motivo del cambio (opcional, máx. 500)
                </label>
                <textarea
                  id="statusReasonInput"
                  rows={2}
                  maxLength={500}
                  value={statusReason}
                  onChange={(e) => setStatusReason(e.target.value)}
                  placeholder="Ej. Revisión y dictamen favorable por perito..."
                  disabled={submittingStatus || !isValidUser}
                  style={{
                    width: '100%',
                    padding: '0.55rem 0.75rem',
                    borderRadius: '6px',
                    border: '1px solid #cbd5e0',
                    fontSize: '0.9rem',
                    resize: 'vertical',
                  }}
                />
              </div>

              <button
                type="submit"
                disabled={!selectedNewStatus || submittingStatus || !isValidUser}
                style={{
                  padding: '0.6rem 1rem',
                  borderRadius: '6px',
                  border: 'none',
                  backgroundColor: !selectedNewStatus || submittingStatus || !isValidUser ? '#a0aec0' : '#0072ce',
                  color: 'white',
                  fontSize: '0.9rem',
                  fontWeight: 600,
                  cursor: !selectedNewStatus || submittingStatus || !isValidUser ? 'not-allowed' : 'pointer',
                  alignSelf: 'flex-start',
                }}
              >
                {submittingStatus ? 'Actualizando...' : 'Aplicar Cambio de Estado'}
              </button>
            </form>
          )}
        </div>
      </div>

      {/* Tabla de Historial Cronológico */}
      <div
        style={{
          backgroundColor: 'white',
          borderRadius: '8px',
          padding: '1.5rem',
          boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
        }}
      >
        <h2 style={{ fontSize: '1.15rem', color: '#002855', margin: '0 0 1rem 0' }}>
          Historial de Estados y Auditoría
        </h2>

        {!claim.history || claim.history.length === 0 ? (
          <p style={{ color: '#718096', fontStyle: 'italic' }}>No hay registros de historial disponibles.</p>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.85rem' }}>
              <thead>
                <tr style={{ backgroundColor: '#f7fafc', borderBottom: '2px solid #edf2f7', color: '#4a5568' }}>
                  <th style={{ padding: '0.75rem 0.85rem' }}>Transición</th>
                  <th style={{ padding: '0.75rem 0.85rem' }}>Motivo</th>
                  <th style={{ padding: '0.75rem 0.85rem' }}>Modificado por</th>
                  <th style={{ padding: '0.75rem 0.85rem' }}>Fecha y hora</th>
                </tr>
              </thead>
              <tbody>
                {claim.history.map((h) => (
                  <tr key={h.id} style={{ borderBottom: '1px solid #edf2f7' }}>
                    <td style={{ padding: '0.75rem 0.85rem', whiteSpace: 'nowrap' }}>
                      {h.previousStatus ? (
                        <span>
                          <StatusBadge status={h.previousStatus} />
                          <span style={{ margin: '0 0.4rem', color: '#718096' }}>→</span>
                        </span>
                      ) : (
                        <span style={{ color: '#718096', marginRight: '0.4rem' }}>Inicial →</span>
                      )}
                      <StatusBadge status={h.newStatus} />
                    </td>
                    <td style={{ padding: '0.75rem 0.85rem', color: '#4a5568' }}>
                      {h.changeReason ?? <span style={{ color: '#a0aec0', fontStyle: 'italic' }}>Sin motivo especificado</span>}
                    </td>
                    <td style={{ padding: '0.75rem 0.85rem', fontWeight: 600, color: '#2d3748' }}>
                      Usuario {h.changedBy}
                    </td>
                    <td style={{ padding: '0.75rem 0.85rem', color: '#718096' }}>
                      {formatDateTime(h.changedAt)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
