import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { Layout } from '../components/Layout'
import { NotFoundPage } from '../pages/NotFoundPage'

// Marcadores de posición que serán completados en los pasos 4, 5 y 6
function ClaimsListPlaceholder() {
  return <div><h2>Listado de Siniestros</h2></div>
}

function ClaimNewPlaceholder() {
  return <div><h2>Nuevo Siniestro</h2></div>
}

function ClaimDetailPlaceholder() {
  return <div><h2>Detalle de Siniestro</h2></div>
}

function ClaimEditPlaceholder() {
  return <div><h2>Editar Siniestro</h2></div>
}

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<ClaimsListPlaceholder />} />
          <Route path="claims/new" element={<ClaimNewPlaceholder />} />
          <Route path="claims/:id" element={<ClaimDetailPlaceholder />} />
          <Route path="claims/:id/edit" element={<ClaimEditPlaceholder />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
