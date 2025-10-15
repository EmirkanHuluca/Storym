import { useState } from 'react'
import { createEntry } from '../api/diary'
import { useNavigate } from 'react-router-dom'

export default function NewEntry() {
  const [title, setTitle] = useState('')
  const [summary, setSummary] = useState('')
  const [content, setContent] = useState('')
  const [date, setDate] = useState(() => new Date().toISOString().slice(0,10))
  const [isHidden, setIsHidden] = useState(false)
  const [files, setFiles] = useState<File[]>([])
  const [error, setError] = useState<string | null>(null)
  const nav = useNavigate()

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    try {
      const dto = await createEntry({ title, summary, content, date, isHidden, files })
      nav(`/entries/${dto.id}`)
    } catch (err: any) {
      setError(err?.response?.data ?? 'Create failed')
    }
  }

  return (
    <form onSubmit={onSubmit} style={{ display: 'grid', gap: 8, maxWidth: 640 }}>
      <h2>New Entry</h2>
      <input placeholder="Title" value={title} onChange={e=>setTitle(e.target.value)} required />
      <textarea placeholder="Summary" value={summary} onChange={e=>setSummary(e.target.value)} required rows={3} />
      <textarea placeholder="Content" value={content} onChange={e=>setContent(e.target.value)} required rows={6} />
      <label>
        Date: <input type="date" value={date} onChange={e=>setDate(e.target.value)} />
      </label>
      <label>
        <input type="checkbox" checked={isHidden} onChange={e=>setIsHidden(e.target.checked)} /> Hidden
      </label>
      <input type="file" multiple onChange={e=>setFiles(Array.from(e.target.files || []))} />
      {error && <div style={{ color: 'crimson' }}>{String(error)}</div>}
      <button type="submit">Create</button>
    </form>
  )
}
