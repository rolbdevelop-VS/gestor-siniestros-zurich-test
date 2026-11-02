import { createContext } from 'react'

export interface UserContextType {
  userId: string
  setUserId: (id: string) => void
  isValidUser: boolean
  validationError: string | null
}

export const UserContext = createContext<UserContextType | undefined>(undefined)
