import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from '../components/Layout'
import { ClaimForm } from '../components/ClaimForm'
import { ClaimDetailPage } from '../pages/ClaimDetailPage'
import { ClaimsListPage } from '../pages/ClaimsListPage'
import { LoginPage } from '../pages/LoginPage'
import { NotFoundPage } from '../pages/NotFoundPage'
import { useUser } from '../context/useUser'
import type { ReactNode } from 'react'

function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useUser()
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }
  return <>{children}</>
}

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route
          path="/"
          element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }
        >
          <Route index element={<ClaimsListPage />} />
          <Route path="claims/new" element={<ClaimForm />} />
          <Route path="claims/:id" element={<ClaimDetailPage />} />
          <Route path="claims/:id/edit" element={<ClaimForm />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
