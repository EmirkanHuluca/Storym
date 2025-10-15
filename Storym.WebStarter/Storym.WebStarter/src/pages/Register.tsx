import { useState } from 'react'
import { register } from '../api/auth'
import { useNavigate } from 'react-router-dom'

export default function Register() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [nick, setNick] = useState('')
  const [error, setError] = useState<string | null>(null)
  const nav = useNavigate()

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    try {
      await register(email, password, nick)
      nav('/login')
    } catch (err: any) {
      setError(err?.response?.data ?? 'Register failed')
    }
  }

  return (
    <form onSubmit={onSubmit} style={{ display: 'grid', gap: 8, maxWidth: 360 }}>
      <h2>Register</h2>
      <input placeholder="Email" value={email} onChange={e=>setEmail(e.target.value)} />
      <input placeholder="Nick" value={nick} onChange={e=>setNick(e.target.value)} />
      <input placeholder="Password" type="password" value={password} onChange={e=>setPassword(e.target.value)} />
      {error && <div style={{ color: 'crimson' }}>{String(error)}</div>}
      <button type="submit">Create Account</button>
    </form>
  )
}
