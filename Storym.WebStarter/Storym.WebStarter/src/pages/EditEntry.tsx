// src/pages/EditEntry.tsx
import { useEffect, useState } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getDetails, editEntry } from '../api/diary'

export default function EditEntry() {
  const { id } = useParams()
  const entryId = Number(id)
  const nav = useNavigate()
  const qc = useQueryClient()

  // Load the entry
  const q = useQuery({
    queryKey: ['entry', entryId],
    queryFn: () => getDetails(entryId),
    enabled: Number.isFinite(entryId),
  })

  // Form state (initialized once from query data)
  const [title, setTitle] = useState('')
  const [summary, setSummary] = useState('')
  const [content, setContent] = useState('')
  const [date, setDate] = useState<string>('')            // yyyy-mm-dd
  const [isHidden, setIsHidden] = useState(false)
  const [newFiles, setNewFiles] = useState<File[]>([])
  const [deleteImages, setDeleteImages] = useState<string[]>([]) // store absolute urls shown in UI
  const [error, setError] = useState<string | null>(null)
  const [hydrated, setHydrated] = useState(false)

  // Hydrate form when data arrives (only once)
  useEffect(() => {
    if (!q.data || hydrated) return
    const e = q.data
    setTitle(e.title)
    setSummary(e.summary)
    setContent(e.content)
    setDate((e.date ?? '').slice(0, 10))
    setIsHidden(e.isHidden)
    setHydrated(true)
  }, [q.data, hydrated])

  if (q.isLoading) return <div className="card">Loading…</div>
  if (q.error || !q.data) return <div className="card">Entry not found</div>

  const images = q.data.imageUrls ?? []

  function toggleDelete(u: string) {
    setDeleteImages((prev) => (prev.includes(u) ? prev.filter(x => x !== u) : [...prev, u]))
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    try {
      await editEntry(entryId, {
        title,
        summary,
        content,
        date,
        isHidden,
        deleteImages,
        newFiles,
      })

      // keep caches fresh
      qc.invalidateQueries({ queryKey: ['entry', entryId] })
      qc.invalidateQueries({ queryKey: ['feed'] })
      qc.invalidateQueries({ queryKey: ['me-posts'] })
      qc.invalidateQueries({ queryKey: ['posts', q.data!.userId] })

      nav(`/entries/${entryId}`)
    } catch (err: any) {
      setError(err?.response?.data ?? 'Save failed')
    }
  }

  return (
    <form onSubmit={onSubmit} style={{ display: 'grid', gap: 8, maxWidth: 640 }}>
      <h2>Edit Entry</h2>

      <input
        placeholder="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        required
      />

      <textarea
        placeholder="Summary"
        value={summary}
        onChange={(e) => setSummary(e.target.value)}
        required
        rows={3}
      />

      <textarea
        placeholder="Content"
        value={content}
        onChange={(e) => setContent(e.target.value)}
        required
        rows={6}
      />

      <label>
        Date:{' '}
        <input
          type="date"
          value={date}
          onChange={(e) => setDate(e.target.value)}
          required
        />
      </label>

      <label>
        <input
          type="checkbox"
          checked={isHidden}
          onChange={(e) => setIsHidden(e.target.checked)}
        />{' '}
        Hidden
      </label>

      {/* Existing images with remove toggles */}
      <div style={{ marginTop: 6 }}>
        <div style={{ fontWeight: 600, marginBottom: 6 }}>Existing images</div>
        {!images.length && <div className="muted">No images</div>}
        {!!images.length && (
          <div className="post-images" style={{ gap: 8 }}>
            {images.map((u, i) => {
              const marked = deleteImages.includes(u)
              return (
                <div key={i} style={{ position: 'relative' }}>
                  <img
                    src={u}
                    alt=""
                    style={{ opacity: marked ? 0.45 : 1, transition: 'opacity .15s' }}
                  />
                  <button
                    type="button"
                    onClick={() => toggleDelete(u)}
                    style={{
                      position: 'absolute',
                      top: 6,
                      right: 6,
                      padding: '4px 8px',
                      borderRadius: 8,
                    }}
                    title={marked ? 'Undo remove' : 'Remove'}
                  >
                    {marked ? 'Undo' : 'Remove'}
                  </button>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Add new images */}
      <input
        type="file"
        multiple
        accept="image/*"
        onChange={(e) => setNewFiles(Array.from(e.target.files || []))}
      />
      {newFiles.length > 0 && (
        <div className="muted">{newFiles.length} file(s) selected</div>
      )}

      {error && <div style={{ color: 'crimson' }}>{String(error)}</div>}

      <div style={{ display: 'flex', gap: 8, marginTop: 6 }}>
        <Link to={`/entries/${entryId}`}><button type="button">Cancel</button></Link>
        <button type="submit">Save changes</button>
      </div>
    </form>
  )
}
