import { useContext } from 'react'
import { UserContext, type UserContextType } from './userContextDef'

export function useUser(): UserContextType {
  const context = useContext(UserContext)
  if (!context) {
    throw new Error('useUser debe ser utilizado dentro de un UserProvider')
  }
  return context
}
