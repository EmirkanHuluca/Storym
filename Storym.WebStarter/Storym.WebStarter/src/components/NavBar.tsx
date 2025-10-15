import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export default function NavBar() {
  const { token, logout } = useAuth()
  const nav = useNavigate()
  return (
    <nav style={{ display: 'flex', gap: 12, alignItems: 'center', marginBottom: 16 }}>
      <Link to="/" style={{ fontWeight: 700 }}>Storym</Link>
      <Link to="/">Feed</Link>
      {token && <Link to="/me">My Posts</Link>}
      {token && <Link to="/new">New Entry</Link>}
      <div style={{ marginLeft: 'auto' }}>
        {!token ? (
          <>
            <Link to="/login">Login</Link>
            <span> • </span>
            <Link to="/register">Register</Link>
          </>
        ) : (
          <button onClick={() => { logout(); nav('/'); }}>
            Logout
          </button>
        )}
      </div>
    </nav>
  )
}
