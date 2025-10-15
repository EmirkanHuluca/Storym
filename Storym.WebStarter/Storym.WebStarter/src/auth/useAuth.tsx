import { createContext, useContext, useEffect, useState } from 'react'

type AuthCtx = {
  token: string | null
  setToken: (t: string | null) => void
  logout: () => void
}

const Ctx = createContext<AuthCtx>({ token: null, setToken: () => {}, logout: () => {} })

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'))
  useEffect(() => {
    if (token) localStorage.setItem('token', token); else localStorage.removeItem('token')
  }, [token])
  const logout = () => setToken(null)
  return <Ctx.Provider value={{ token, setToken, logout }}>{children}</Ctx.Provider>
}

export function useAuth() {
  return useContext(Ctx)
}
