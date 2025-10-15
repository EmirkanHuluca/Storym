import type { DiaryEntryDto } from '../types'
import Avatar from './Avatar'
import { toggleLike } from '../api/diary'
import { followUser, unfollowUser, getFollowState } from '../api/users'
import { useAuth } from '../auth/useAuth'
import { useEffect, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useImageViewer } from './ImageViewer'
import { Link } from 'react-router-dom'

export default function PostCard({
  entry,
  currentUserId,
  onChanged
}: { entry: DiaryEntryDto; currentUserId: string; onChanged?: () => void }) {
  const { open } = useImageViewer()
  const { token } = useAuth()
  const qc = useQueryClient()
  const isMe = !!currentUserId && currentUserId === entry.userId
  const canShowFollow = !!token && !isMe && (entry.ownerFollowing === false || entry.ownerFollowing === undefined)

  const [following, setFollowing] = useState<boolean | null>(null)
  const canFollow = !!token && !isMe
  const isOwner = currentUserId === entry.userId

  useEffect(() => {
    let alive = true
    if (!canFollow) { setFollowing(null); return }
    ;(async () => {
      try {
        const f = await getFollowState(entry.userId)
        if (alive) setFollowing(f)
      } catch {
        if (alive) setFollowing(false)
      }
    })()
    return () => { alive = false }
  }, [entry.userId, canFollow])

  async function followOwner() {
    await followUser(entry.userId)

    // Optimistically mark this post's owner as followed in FE caches
    qc.setQueriesData({ queryKey: ['feed'] }, (old: unknown) => {
      if (!Array.isArray(old)) return old
      return old.map((e: any) =>
        e.userId === entry.userId ? { ...e, ownerFollowing: true } : e
      )
    })
    qc.setQueriesData({ queryKey: ['posts', entry.userId] }, (old: unknown) => {
      if (!Array.isArray(old)) return old
      return old.map((e: any) => ({ ...e, ownerFollowing: true }))
    })

    // Keep other bits in sync
    await qc.invalidateQueries({ queryKey: ['suggested'] })
    await qc.refetchQueries({ queryKey: ['suggested'], type: 'active' })
    await qc.invalidateQueries({ queryKey: ['profile', entry.userId] })
    await qc.invalidateQueries({ queryKey: ['me-profile'] })
  }

  return (
    <article className="card">
      <header className="post-header">
        <Link to={`/u/${entry.userId}`} title="View profile">
    <Avatar src={entry.ownerAvatarUrl ?? undefined} />
  </Link>
        <div className="nick-row">
          <Link to={`/u/${entry.userId}`} className="nick">{entry.ownerNick}</Link>
          <span className="email">{entry.ownerEmail}</span>
          {canShowFollow && (
            <button onClick={followOwner} style={{ marginLeft: 'auto' }}>
              Follow
            </button>
          )}
        </div>
      </header>

      <div className="title">{entry.title}</div>

      {entry.imageUrls?.length > 0 && (
        <div className="post-images">
          {entry.imageUrls.slice(0,3).map((u, i) => (
            <img key={i} src={u} alt="" onClick={() => open(entry.imageUrls, i)}    // <— open gallery starting at clicked one
              style={{ cursor: 'zoom-in' }}/>
          ))}
        </div>
      )}

      <div className="summary">{entry.summary}</div>

      <div className="actions">
        <button onClick={async () => { await toggleLike(entry.id); onChanged?.() }}>
          ❤️ Like ({entry.likeCount})
        </button>
        <button>💬 Comment</button>
        <button onClick={() => navigator.share?.({ title: entry.title, url: location.href }).catch(()=>{})}>
          🔗 Share
        </button>
        {isOwner && (
          <a href={`/entries/${entry.id}/edit`} style={{ marginLeft: 'auto' }}>
            <button type="button">✏️ Edit</button>
          </a>
        )}
      </div>
    </article>
  )
}
