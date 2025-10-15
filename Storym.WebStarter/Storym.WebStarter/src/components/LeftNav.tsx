import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

function NavLink({ to, children }: { to: string; children: React.ReactNode }) {
  const loc = useLocation()
  const active = loc.pathname === to
  return (
    <Link to={to} style={{
      display: 'block', padding: '10px 12px', margin: '4px 0',
      borderRadius: 10, border: '1px solid', borderColor: active ? '#2dd4bf55' : 'transparent',
      background: active ? '#1b1f24' : 'transparent'
    }}>
      {children}
    </Link>
  )
}

export default function LeftNav() {
  const { token, logout } = useAuth()
  const nav = useNavigate()
  return (
    <>
      <h1>Storym</h1>
      <nav>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/search">Search</NavLink>
        <NavLink to="/explore">Explore</NavLink>
        <NavLink to="/new">New Entry</NavLink>
        <NavLink to="/me">Profile</NavLink>
        <div style={{ height: 8 }} />
        {token ? (
          <button onClick={() => { logout(); nav('/'); }}>Logout</button>
        ) : (
          <div style={{ display: 'flex', gap: 8 }}>
            <Link to="/login">Login</Link>
            <Link to="/register">Register</Link>
          </div>
        )}
      </nav>
    </>
  )
}
