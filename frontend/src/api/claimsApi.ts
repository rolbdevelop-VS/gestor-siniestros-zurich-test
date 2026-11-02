import type {
  ChangeStatusRequest,
  ClaimDetailResponse,
  ClaimStatus,
  ClaimSummaryResponse,
  CreateClaimRequest,
  UpdateClaimRequest,
} from '../types'
import { createNetworkError, parseApiError } from './errors'

const BASE_URL = (import.meta.env.VITE_API_BASE_URL as string) || 'http://localhost:5137'

let currentUserIdProvider = (): string => {
  try {
    return localStorage.getItem('zurich_claims_user_id') || ''
  } catch {
    return ''
  }
}

export function setApiUserIdProvider(provider: () => string): void {
  currentUserIdProvider = provider
}

export function getApiBaseUrl(): string {
  return BASE_URL
}

interface RequestOptions {
  method?: string
  body?: unknown
  signal?: AbortSignal
  userId?: string
}

async function request<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, signal, userId } = options
  const activeUserId = (userId ?? currentUserIdProvider()).trim()

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  }

  if (activeUserId) {
    headers['x-usuario'] = activeUserId
  }

  const url = `${BASE_URL}${endpoint}`

  let response: Response
  try {
    response = await fetch(url, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal,
    })
  } catch (err: unknown) {
    if (err instanceof DOMException && err.name === 'AbortError') {
      throw err
    }
    throw createNetworkError(err)
  }

  if (response.status === 204) {
    return undefined as unknown as T
  }

  if (!response.ok) {
    throw await parseApiError(response)
  }

  try {
    return (await response.json()) as T
  } catch {
    return undefined as unknown as T
  }
}

export const claimsApi = {
  getAll: (status?: ClaimStatus, policyNumber?: string, signal?: AbortSignal, userId?: string): Promise<ClaimSummaryResponse[]> => {
    const params = new URLSearchParams()
    if (status) {
      params.append('status', status)
    }
    if (policyNumber && policyNumber.trim()) {
      params.append('policyNumber', policyNumber.trim())
    }
    const query = params.toString() ? `?${params.toString()}` : ''
    return request<ClaimSummaryResponse[]>(`/api/claims${query}`, { method: 'GET', signal, userId })
  },

  getById: (id: number, signal?: AbortSignal, userId?: string): Promise<ClaimDetailResponse> => {
    return request<ClaimDetailResponse>(`/api/claims/${id}`, { method: 'GET', signal, userId })
  },

  create: (data: CreateClaimRequest, signal?: AbortSignal, userId?: string): Promise<ClaimDetailResponse> => {
    return request<ClaimDetailResponse>('/api/claims', { method: 'POST', body: data, signal, userId })
  },

  update: (id: number, data: UpdateClaimRequest, signal?: AbortSignal, userId?: string): Promise<ClaimSummaryResponse> => {
    return request<ClaimSummaryResponse>(`/api/claims/${id}`, { method: 'PUT', body: data, signal, userId })
  },

  changeStatus: (id: number, data: ChangeStatusRequest, signal?: AbortSignal, userId?: string): Promise<ClaimDetailResponse> => {
    return request<ClaimDetailResponse>(`/api/claims/${id}/status`, { method: 'POST', body: data, signal, userId })
  },

  delete: (id: number, signal?: AbortSignal, userId?: string): Promise<void> => {
    return request<void>(`/api/claims/${id}`, { method: 'DELETE', signal, userId })
  },
}
