import { useEffect, useState, type ReactNode } from 'react'
import { setApiUserIdProvider } from '../api/claimsApi'
import { UserContext } from './userContextDef'
import { validateUserId } from './userUtils'

const STORAGE_USER_KEY = 'zurich_claims_user_id'
const STORAGE_AUTH_KEY = 'zurich_claims_auth'

export function UserProvider({ children }: { children: ReactNode }) {
  const [userId, setUserIdState] = useState<string>(() => {
    try {
      return localStorage.getItem(STORAGE_USER_KEY) ?? '1001'
    } catch {
      return '1001'
    }
  })

  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(() => {
    try {
      return localStorage.getItem(STORAGE_AUTH_KEY) === 'true'
    } catch {
      return false
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
      localStorage.setItem(STORAGE_USER_KEY, newId)
    } catch {
      // Ignorar errores de storage
    }
  }

  const login = (newId: string) => {
    setUserId(newId)
    setIsAuthenticated(true)
    try {
      localStorage.setItem(STORAGE_AUTH_KEY, 'true')
    } catch {
      // Ignorar
    }
  }

  const logout = () => {
    setIsAuthenticated(false)
    try {
      localStorage.removeItem(STORAGE_AUTH_KEY)
    } catch {
      // Ignorar
    }
  }

  return (
    <UserContext.Provider
      value={{
        userId,
        setUserId,
        isValidUser,
        validationError,
        isAuthenticated,
        login,
        logout,
      }}
    >
      {children}
    </UserContext.Provider>
  )
}
