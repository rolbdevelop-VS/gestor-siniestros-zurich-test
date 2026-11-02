import { UserProvider } from './context/UserContext'
import { AppRoutes } from './routes/AppRoutes'

export default function App() {
  return (
    <UserProvider>
      <AppRoutes />
    </UserProvider>
  )
}
