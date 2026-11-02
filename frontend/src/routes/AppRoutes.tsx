import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { Layout } from '../components/Layout'
import { ClaimForm } from '../components/ClaimForm'
import { ClaimDetailPage } from '../pages/ClaimDetailPage'
import { ClaimsListPage } from '../pages/ClaimsListPage'
import { NotFoundPage } from '../pages/NotFoundPage'

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
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
