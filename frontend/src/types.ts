export type ClaimStatus = 'Open' | 'InReview' | 'Closed'

export interface ClaimSummaryResponse {
  id: number
  policyNumber: string
  insuredName: string
  claimType: string
  estimatedAmount: number
  currentStatus: ClaimStatus
  createdAt: string
  updatedAt: string | null
  createdBy: string
  updatedBy: string | null
}

export interface ClaimStatusHistoryResponse {
  id: number
  previousStatus: ClaimStatus | null
  newStatus: ClaimStatus
  changeReason: string | null
  changedAt: string
  changedBy: string
}

export interface ClaimDetailResponse {
  id: number
  policyNumber: string
  insuredName: string
  claimType: string
  estimatedAmount: number
  currentStatus: ClaimStatus
  createdAt: string
  updatedAt: string | null
  createdBy: string
  updatedBy: string | null
  history: ClaimStatusHistoryResponse[]
}

export interface CreateClaimRequest {
  policyNumber: string
  insuredName: string
  claimType: string
  estimatedAmount: number
}

export interface UpdateClaimRequest {
  policyNumber: string
  insuredName: string
  claimType: string
  estimatedAmount: number
}

export interface ChangeStatusRequest {
  newStatus: ClaimStatus
  reason?: string | null
}

export interface ProblemDetails {
  type?: string | null
  title?: string | null
  status?: number | null
  detail?: string | null
  instance?: string | null
  errors?: Record<string, string[]>
}
