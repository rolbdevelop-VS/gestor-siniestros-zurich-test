import { useEffect, useState, type ReactNode } from 'react'
import { setApiUserIdProvider } from '../api/claimsApi'
import { UserContext } from './userContextDef'
import { validateUserId } from './userUtils'

const STORAGE_KEY = 'zurich_claims_user_id'

export function UserProvider({ children }: { children: ReactNode }) {
  const [userId, setUserIdState] = useState<string>(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY)
      return stored ?? '1001'
    } catch {
      return '1001'
    }
  })

  const validationError = validateUserId(userId)
  const isValidUser = validationError === null

  useEffect(() => {
    setApiUserIdProvider(() => userId.trim())
  }, [userId])

  const setUserId = (newId: string) => {
    setUserIdState(newId)
    try {
      localStorage.setItem(STORAGE_KEY, newId)
    } catch {
      // Ignorar errores de storage
    }
  }

  return (
    <UserContext.Provider value={{ userId, setUserId, isValidUser, validationError }}>
      {children}
    </UserContext.Provider>
  )
}
