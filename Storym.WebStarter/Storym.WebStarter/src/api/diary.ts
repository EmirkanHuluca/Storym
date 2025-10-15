import api from './client'
import type { DiaryEntryDto } from '../types'
import { abs, rel } from '../lib/absUrl'

function massage<T extends DiaryEntryDto>(dto: T): T {
  return { ...dto, 
    imageUrls: (dto.imageUrls || []).map(abs), 
    ownerAvatarUrl: dto.ownerAvatarUrl ? abs(dto.ownerAvatarUrl) : dto.ownerAvatarUrl 
  } as T
}

export async function getByUser(userId: string): Promise<DiaryEntryDto[]> {
  const { data } = await api.get(`/api/diary/by-user/${userId}`)
  return (data as DiaryEntryDto[]).map(massage)
}
export async function getFeed(): Promise<DiaryEntryDto[]> {
  const { data } = await api.get('/api/diary/feed')
  return data.map(massage)
}

export async function getMine(): Promise<DiaryEntryDto[]> {
  const { data } = await api.get('/api/diary/me')
  return data.map(massage)
}

export async function getDetails(id: number): Promise<DiaryEntryDto> {
  const { data } = await api.get(`/api/diary/${id}`)
  return massage(data)
}

export async function toggleLike(id: number) {
  const { data } = await api.post(`/api/diary/${id}/like`)
  return data.likes as number
}

export async function createEntry(input: {
  title: string
  summary: string
  content: string
  date: string
  isHidden: boolean
  files: File[]
}): Promise<DiaryEntryDto> {
  const fd = new FormData()
  fd.append('Title', input.title)
  fd.append('Summary', input.summary)
  fd.append('Content', input.content)
  fd.append('Date', input.date)
  fd.append('IsHidden', String(input.isHidden))
  input.files.forEach(f => fd.append('files', f, f.name))
  const { data } = await api.post('/api/diary', fd, { headers: { 'Content-Type': 'multipart/form-data' } })
  return massage(data)
}

export async function editEntry(
  id: number,
  input: {
    title: string
    summary: string
    content: string
    date: string | Date
    isHidden: boolean
    deleteImages: string[]           // absolute or relative; we’ll normalize
    newFiles: File[]
  }
): Promise<DiaryEntryDto> {
  const fd = new FormData()
  fd.append('Title', input.title)
  fd.append('Summary', input.summary)
  fd.append('Content', input.content)
  fd.append('Date', typeof input.date === 'string' ? input.date : input.date.toISOString())
  fd.append('IsHidden', String(input.isHidden))

  // server expects semicolon-separated relative paths (Program.cs reads "DeleteImages")
  const rels = (input.deleteImages || []).map(rel).filter(Boolean)
  fd.append('DeleteImages', rels.join(';'))

  for (const f of input.newFiles || []) {
    fd.append('files', f) // key name doesn’t matter; API reads all form files
  }

  const { data } = await api.put(`/api/diary/${id}`, fd)
  return massage(data as DiaryEntryDto)
}
