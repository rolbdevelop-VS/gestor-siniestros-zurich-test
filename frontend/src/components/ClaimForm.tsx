import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { claimsApi } from '../api/claimsApi'
import { ApiError } from '../api/errors'
import { ErrorMessage } from './ErrorMessage'
import { useUser } from '../context/useUser'
import type { CreateClaimRequest, UpdateClaimRequest } from '../types'
import { validateAmount } from '../utils/validators'

interface FormFieldErrors {
  policyNumber?: string
  insuredName?: string
  claimType?: string
  estimatedAmount?: string
}

export function ClaimForm() {
  const { id } = useParams<{ id?: string }>()
  const navigate = useNavigate()
  const { isValidUser } = useUser()

  const isEditMode = Boolean(id)
  const claimId = id ? parseInt(id, 10) : undefined

  const [policyNumber, setPolicyNumber] = useState<string>('')
  const [insuredName, setInsuredName] = useState<string>('')
  const [claimType, setClaimType] = useState<string>('')
  const [estimatedAmountStr, setEstimatedAmountStr] = useState<string>('')

  const [loadingInitial, setLoadingInitial] = useState<boolean>(false)
  const [submitting, setSubmitting] = useState<boolean>(false)
  const [clientErrors, setClientErrors] = useState<FormFieldErrors>({})
  const [serverError, setServerError] = useState<unknown | null>(null)

  // En modo edición, precargar los datos del siniestro
  useEffect(() => {
    if (!isEditMode || !claimId || !isValidUser) {
      return
    }

    const controller = new AbortController()
    let isMounted = true

    async function fetchClaim() {
      try {
        setLoadingInitial(true)
        setServerError(null)
        const claim = await claimsApi.getById(claimId!, controller.signal)
        if (isMounted) {
          setPolicyNumber(claim.policyNumber)
          setInsuredName(claim.insuredName)
          setClaimType(claim.claimType)
          setEstimatedAmountStr(claim.estimatedAmount.toString())
          setLoadingInitial(false)
        }
      } catch (err: unknown) {
        if (err instanceof DOMException && err.name === 'AbortError') {
          return
        }
        if (isMounted) {
          setServerError(err)
          setLoadingInitial(false)
        }
      }
    }

    void fetchClaim()

    return () => {
      isMounted = false
      controller.abort()
    }
  }, [isEditMode, claimId, isValidUser])

  const validateForm = (): { isValid: boolean; amount?: number } => {
    const errors: FormFieldErrors = {}

    const trimmedPolicy = policyNumber.trim()
    if (!trimmedPolicy) {
      errors.policyNumber = 'El número de póliza es obligatorio.'
    } else if (trimmedPolicy.length > 50) {
      errors.policyNumber = 'El número de póliza no puede exceder 50 caracteres.'
    }

    const trimmedInsured = insuredName.trim()
    if (!trimmedInsured) {
      errors.insuredName = 'El nombre del asegurado es obligatorio.'
    } else if (trimmedInsured.length > 150) {
      errors.insuredName = 'El nombre del asegurado no puede exceder 150 caracteres.'
    }

    const trimmedType = claimType.trim()
    if (!trimmedType) {
      errors.claimType = 'El tipo de siniestro es obligatorio.'
    } else if (trimmedType.length > 100) {
      errors.claimType = 'El tipo de siniestro no puede exceder 100 caracteres.'
    }

    const amountValidation = validateAmount(estimatedAmountStr)
    if (!amountValidation.isValid) {
      errors.estimatedAmount = amountValidation.error
    }

    setClientErrors(errors)
    return {
      isValid: Object.keys(errors).length === 0,
      amount: amountValidation.amount,
    }
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (submitting || !isValidUser) return

    setServerError(null)
    const { isValid, amount } = validateForm()
    if (!isValid || amount === undefined) {
      return
    }

    setSubmitting(true)

    try {
      if (isEditMode && claimId) {
        const updateData: UpdateClaimRequest = {
          policyNumber: policyNumber.trim(),
          insuredName: insuredName.trim(),
          claimType: claimType.trim(),
          estimatedAmount: amount,
        }
        await claimsApi.update(claimId, updateData)
        navigate(`/claims/${claimId}`)
      } else {
        const createData: CreateClaimRequest = {
          policyNumber: policyNumber.trim(),
          insuredName: insuredName.trim(),
          claimType: claimType.trim(),
          estimatedAmount: amount,
        }
        const created = await claimsApi.create(createData)
        navigate(`/claims/${created.id}`)
      }
    } catch (err: unknown) {
      setServerError(err)
      setSubmitting(false)
    }
  }

  // Extraer errores por campo devueltos por la API si existen
  const serverFieldErrors: Record<string, string> = {}
  if (serverError instanceof ApiError && serverError.errors) {
    for (const [key, msgs] of Object.entries(serverError.errors)) {
      const lower = key.toLowerCase()
      if (lower.includes('policy')) serverFieldErrors.policyNumber = msgs.join(', ')
      if (lower.includes('insured')) serverFieldErrors.insuredName = msgs.join(', ')
      if (lower.includes('claimtype')) serverFieldErrors.claimType = msgs.join(', ')
      if (lower.includes('amount')) serverFieldErrors.estimatedAmount = msgs.join(', ')
    }
  }

  if (loadingInitial) {
    return (
      <div style={{ textAlign: 'center', padding: '3rem', color: '#718096' }}>
        <p style={{ fontSize: '1.1rem' }}>Cargando datos del siniestro...</p>
      </div>
    )
  }

  return (
    <div style={{ maxWidth: '650px', margin: '0 auto' }}>
      <div style={{ marginBottom: '1.5rem' }}>
        <Link
          to={isEditMode && claimId ? `/claims/${claimId}` : '/'}
          style={{ color: '#0072ce', textDecoration: 'none', fontSize: '0.9rem', fontWeight: 500 }}
        >
          ← Volver {isEditMode ? 'al detalle' : 'al listado'}
        </Link>
        <h1 style={{ fontSize: '1.75rem', color: '#002855', margin: '0.5rem 0 0.25rem 0' }}>
          {isEditMode ? `Editar Siniestro #${claimId}` : 'Registrar Nuevo Siniestro'}
        </h1>
        <p style={{ color: '#718096', margin: 0, fontSize: '0.95rem' }}>
          {isEditMode
            ? 'Actualiza los datos del siniestro. El estado se modifica de forma independiente en el detalle.'
            : 'Ingresa los datos correspondientes para dar de alta el siniestro en estado Abierto.'}
        </p>
      </div>

      {Boolean(serverError) && <ErrorMessage error={serverError} onDismiss={() => setServerError(null)} />}

      <form
        onSubmit={handleSubmit}
        noValidate
        style={{
          backgroundColor: 'white',
          padding: '2rem',
          borderRadius: '8px',
          boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
          display: 'flex',
          flexDirection: 'column',
          gap: '1.25rem',
        }}
      >
        {/* Número de Póliza */}
        <div>
          <label
            htmlFor="policyNumber"
            style={{ display: 'block', fontSize: '0.9rem', fontWeight: 600, color: '#2d3748', marginBottom: '0.35rem' }}
          >
            Número de póliza <span style={{ color: '#e53e3e' }}>*</span>
          </label>
          <input
            id="policyNumber"
            type="text"
            maxLength={50}
            value={policyNumber}
            onChange={(e) => setPolicyNumber(e.target.value)}
            placeholder="Ej. POL-2024-001"
            disabled={submitting || !isValidUser}
            style={{
              width: '100%',
              padding: '0.6rem 0.75rem',
              borderRadius: '6px',
              border: `1px solid ${clientErrors.policyNumber || serverFieldErrors.policyNumber ? '#e53e3e' : '#cbd5e0'}`,
              fontSize: '0.95rem',
            }}
          />
          {(clientErrors.policyNumber || serverFieldErrors.policyNumber) && (
            <p style={{ margin: '0.3rem 0 0', fontSize: '0.8rem', color: '#e53e3e' }}>
              {clientErrors.policyNumber || serverFieldErrors.policyNumber}
            </p>
          )}
        </div>

        {/* Nombre del Asegurado */}
        <div>
          <label
            htmlFor="insuredName"
            style={{ display: 'block', fontSize: '0.9rem', fontWeight: 600, color: '#2d3748', marginBottom: '0.35rem' }}
          >
            Nombre del asegurado <span style={{ color: '#e53e3e' }}>*</span>
          </label>
          <input
            id="insuredName"
            type="text"
            maxLength={150}
            value={insuredName}
            onChange={(e) => setInsuredName(e.target.value)}
            placeholder="Ej. Ana Gómez Martínez"
            disabled={submitting || !isValidUser}
            style={{
              width: '100%',
              padding: '0.6rem 0.75rem',
              borderRadius: '6px',
              border: `1px solid ${clientErrors.insuredName || serverFieldErrors.insuredName ? '#e53e3e' : '#cbd5e0'}`,
              fontSize: '0.95rem',
            }}
          />
          {(clientErrors.insuredName || serverFieldErrors.insuredName) && (
            <p style={{ margin: '0.3rem 0 0', fontSize: '0.8rem', color: '#e53e3e' }}>
              {clientErrors.insuredName || serverFieldErrors.insuredName}
            </p>
          )}
        </div>

        {/* Tipo de Siniestro */}
        <div>
          <label
            htmlFor="claimType"
            style={{ display: 'block', fontSize: '0.9rem', fontWeight: 600, color: '#2d3748', marginBottom: '0.35rem' }}
          >
            Tipo de siniestro <span style={{ color: '#e53e3e' }}>*</span>
          </label>
          <input
            id="claimType"
            type="text"
            maxLength={100}
            value={claimType}
            onChange={(e) => setClaimType(e.target.value)}
            placeholder="Ej. Colisión vehicular frontal"
            disabled={submitting || !isValidUser}
            style={{
              width: '100%',
              padding: '0.6rem 0.75rem',
              borderRadius: '6px',
              border: `1px solid ${clientErrors.claimType || serverFieldErrors.claimType ? '#e53e3e' : '#cbd5e0'}`,
              fontSize: '0.95rem',
            }}
          />
          {(clientErrors.claimType || serverFieldErrors.claimType) && (
            <p style={{ margin: '0.3rem 0 0', fontSize: '0.8rem', color: '#e53e3e' }}>
              {clientErrors.claimType || serverFieldErrors.claimType}
            </p>
          )}
        </div>

        {/* Monto Estimado */}
        <div>
          <label
            htmlFor="estimatedAmount"
            style={{ display: 'block', fontSize: '0.9rem', fontWeight: 600, color: '#2d3748', marginBottom: '0.35rem' }}
          >
            Monto estimado (MXN) <span style={{ color: '#e53e3e' }}>*</span>
          </label>
          <input
            id="estimatedAmount"
            type="text"
            inputMode="decimal"
            value={estimatedAmountStr}
            onChange={(e) => setEstimatedAmountStr(e.target.value)}
            placeholder="Ej. 18500.50"
            disabled={submitting || !isValidUser}
            style={{
              width: '100%',
              padding: '0.6rem 0.75rem',
              borderRadius: '6px',
              border: `1px solid ${clientErrors.estimatedAmount || serverFieldErrors.estimatedAmount ? '#e53e3e' : '#cbd5e0'}`,
              fontSize: '0.95rem',
            }}
          />
          <small style={{ display: 'block', marginTop: '0.25rem', color: '#718096', fontSize: '0.75rem' }}>
            Número positivo mayor a cero, con un máximo de 2 decimales.
          </small>
          {(clientErrors.estimatedAmount || serverFieldErrors.estimatedAmount) && (
            <p style={{ margin: '0.3rem 0 0', fontSize: '0.8rem', color: '#e53e3e' }}>
              {clientErrors.estimatedAmount || serverFieldErrors.estimatedAmount}
            </p>
          )}
        </div>

        {/* Botones de Acción */}
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1rem' }}>
          <Link
            to={isEditMode && claimId ? `/claims/${claimId}` : '/'}
            style={{
              padding: '0.6rem 1.2rem',
              borderRadius: '6px',
              border: '1px solid #cbd5e0',
              backgroundColor: '#edf2f7',
              color: '#4a5568',
              textDecoration: 'none',
              fontSize: '0.95rem',
              fontWeight: 500,
            }}
          >
            Cancelar
          </Link>

          <button
            type="submit"
            disabled={submitting || !isValidUser}
            style={{
              padding: '0.6rem 1.4rem',
              borderRadius: '6px',
              border: 'none',
              backgroundColor: submitting || !isValidUser ? '#a0aec0' : '#0072ce',
              color: 'white',
              fontSize: '0.95rem',
              fontWeight: 600,
              cursor: submitting || !isValidUser ? 'not-allowed' : 'pointer',
              boxShadow: '0 1px 2px rgba(0,0,0,0.05)',
            }}
          >
            {submitting ? 'Guardando...' : isEditMode ? 'Guardar Cambios' : 'Registrar Siniestro'}
          </button>
        </div>
      </form>
    </div>
  )
}
