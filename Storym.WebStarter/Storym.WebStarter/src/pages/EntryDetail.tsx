import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getDetails, toggleLike } from '../api/diary'
import { timeAgo } from '../lib/timeAgo'
import { useAuth } from '../auth/useAuth'

export default function EntryDetail() {
  const { id } = useParams()
  const q = useQuery({ queryKey: ['entry', id], queryFn: () => getDetails(Number(id)) })
  const { token } = useAuth()
  if (q.isLoading) return <div>Loading...</div>
  if (q.error) return <div>Error</div>
  const e = q.data!
  return (
    <article>
      <h2>{e.title}</h2>
      <small>{timeAgo(e.date)}</small>
      <p><i>{e.summary}</i></p>
      <div style={{ whiteSpace: 'pre-wrap' }}>{e.content}</div>
      {e.imageUrls?.length > 0 && (
        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginTop: 8 }}>
          {e.imageUrls.map((u,i) => (
            <img key={i} src={u} alt="" style={{ width: 160, height: 160, objectFit: 'cover', borderRadius: 4 }} />
          ))}
        </div>
      )}
      <div style={{ marginTop: 12 }}>
        <button disabled={!token} onClick={async () => { await toggleLike(e.id); q.refetch() }}>
          {e.likedByMe ? 'Unlike' : 'Like'} ({e.likeCount})
        </button>
      </div>
    </article>
  )
}
