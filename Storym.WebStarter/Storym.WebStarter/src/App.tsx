import { Routes, Route, Navigate, Link,Outlet } from 'react-router-dom'
import Login from './pages/Login'
import Register from './pages/Register'
import Feed from './pages/Feed'
import MyPosts from './pages/MyPosts'
import NewEntry from './pages/NewEntry'
import EntryDetail from './pages/EntryDetail'
import { useAuth } from './auth/useAuth'
import LeftNav from './components/LeftNav'
import RightRail from './components/RightRail'
import VisitProfile from './pages/VisitProfile'
import EditEntry from './pages/EditEntry'

function Protected({ children }: { children: JSX.Element }) {
  const { token } = useAuth()
  if (!token) return <Navigate to="/login" replace />
  return children
}
function Shell() {
  return (
    <div className="app-shell">
      <aside className="left-rail"><LeftNav /></aside>
      <main className="center-col"><Outlet /></main>
      <aside className="right-rail"><RightRail /></aside>
    </div>
  )
}
export default function App() {
  return (
    <Routes>
      <Route element={<Shell />}>
        <Route path="/" element={<Feed />} />
        <Route path="/entries/:id" element={<EntryDetail />} />
        <Route path="/me" element={<Protected><MyPosts /></Protected>} />
        <Route path="/new" element={<Protected><NewEntry /></Protected>} />
        <Route path="/u/:id" element={<VisitProfile />} />
        <Route path="/entries/:id/edit" element={<Protected><EditEntry /></Protected>} />
      </Route>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  )
}
