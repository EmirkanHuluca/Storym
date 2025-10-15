import api from './client'

export async function register(email: string, password: string, nick: string) {
  await api.post('/auth/register', { email, password, nick })
}

export async function login(email: string, password: string): Promise<string> {
  const { data } = await api.post('/auth/login', { email, password })
  return data.token as string
}
