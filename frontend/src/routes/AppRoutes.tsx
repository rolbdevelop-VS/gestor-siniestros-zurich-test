import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { Layout } from '../components/Layout'
import { ClaimForm } from '../components/ClaimForm'
import { ClaimsListPage } from '../pages/ClaimsListPage'
import { NotFoundPage } from '../pages/NotFoundPage'

// Marcador de posición que será completado en el paso 6
function ClaimDetailPlaceholder() {
  return <div><h2>Detalle de Siniestro</h2></div>
}

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<ClaimsListPage />} />
          <Route path="claims/new" element={<ClaimForm />} />
          <Route path="claims/:id" element={<ClaimDetailPlaceholder />} />
          <Route path="claims/:id/edit" element={<ClaimForm />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
