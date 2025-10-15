import api from './client'
import { abs } from '../lib/absUrl'



export type MyProfile = {
  id: string; nick: string; email: string; avatarUrl?: string | null;
  entryCount: number; followerCount: number; followingCount: number;
}
export type PublicUserProfile = {
  id: string; nick: string; email: string; avatarUrl?: string | null;
  entryCount: number; followerCount: number; followingCount: number;
  isMe: boolean; following: boolean;
}
export async function getMyProfile(): Promise<MyProfile> {
  const { data } = await api.get('/users/me')
  return data
}
export async function getUserProfile(id: string): Promise<PublicUserProfile> {
  const { data } = await api.get(`/users/${id}`)
  if (data?.avatarUrl) data.avatarUrl = abs(data.avatarUrl)
  return data
}
export async function uploadAvatar(file: File): Promise<string> {
  const fd = new FormData()
  fd.append('file', file)
  const { data } = await api.post('/users/me/avatar', fd, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return data.avatarUrl as string
}

export type UserMini = { id: string; nick: string; email: string; avatarUrl?: string; following?: boolean }
export type Paged<T> = { items: T[]; total: number; page: number; pageSize: number; hasMore: boolean }
const fix = (u: UserMini): UserMini => ({ ...u, avatarUrl: u.avatarUrl ? abs(u.avatarUrl) : u.avatarUrl })

export async function getFollowers(id: string, page = 1, pageSize = 20): Promise<Paged<UserMini>> {
  const { data } = await api.get(`/users/${id}/followers`, { params: { page, pageSize } })
  return { ...data, items: (data.items as UserMini[]).map(fix) }
}

export async function getFollowing(id: string, page = 1, pageSize = 20): Promise<Paged<UserMini>> {
  const { data } = await api.get(`/users/${id}/following`, { params: { page, pageSize } })
  return { ...data, items: (data.items as UserMini[]).map(fix) }
}

function massage(u: UserMini): UserMini {
  return { ...u, avatarUrl: u.avatarUrl ? abs(u.avatarUrl) : u.avatarUrl }
}

export async function getSuggestedUsers(take = 5): Promise<UserMini[]> {
  const { data } = await api.get('/users/suggested', { params: { take } })
  return (data as UserMini[]).map(massage)
}

export async function searchUsers(q: string, take = 10): Promise<UserMini[]> {
  const { data } = await api.get('/users/search', { params: { q, take } })
  return (data as UserMini[]).map(massage)
}
export async function followUser(id: string) {
  await api.post(`/users/${id}/follow`)
  return true
}

export async function unfollowUser(id: string) {
  await api.delete(`/users/${id}/follow`)
  return true
}

export async function getFollowState(id: string): Promise<boolean> {
  const { data } = await api.get(`/users/${id}/follow-state`)
  return Boolean(data.following)
}
