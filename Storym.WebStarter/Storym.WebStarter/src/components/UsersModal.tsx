import { useEffect, useState } from 'react'
import Modal from './Modal'
import Avatar from './Avatar'
import { followUser, unfollowUser, getFollowers, getFollowing, type UserMini } from '../api/users'

export default function UsersModal({
  open, onClose, userId, kind
}: { open: boolean; onClose: () => void; userId: string; kind: 'followers' | 'following' }) {

  const [page, setPage] = useState(1)
  const [items, setItems] = useState<UserMini[]>([])
  const [hasMore, setHasMore] = useState(false)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!open) return
    setPage(1); setItems([]); setHasMore(false)
    void load(1)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, userId, kind])

  async function load(p: number) {
    setLoading(true)
    try {
      const res = kind === 'followers' ? await getFollowers(userId, p, 20) : await getFollowing(userId, p, 20)
      setItems(prev => p === 1 ? res.items : [...prev, ...res.items])
      setHasMore(res.hasMore)
      setPage(res.page)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Modal open={open} onClose={onClose} title={kind === 'followers' ? 'Followers' : 'Following'}>
      <div style={{ display:'grid', gap: 8, maxHeight: '60vh', overflow: 'auto' }}>
        {items.map(u => <Row key={u.id} u={u} />)}
        {items.length === 0 && !loading && <div style={{color:'var(--muted)'}}>No users.</div>}
        {loading && <div>Loading…</div>}
      </div>
      {hasMore && !loading && (
        <div style={{ marginTop: 10, textAlign:'center' }}>
          <button onClick={() => load(page + 1)}>Load more</button>
        </div>
      )}
    </Modal>
  )
}

function Row({ u }: { u: UserMini }) {
  const [following, setFollowing] = useState<boolean>(u.following ?? false)
  const [busy, setBusy] = useState(false)
  return (
    <div className="suggested-item">
      <Avatar src={u.avatarUrl || undefined} />
      <div>
        <div className="nick">{u.nick}</div>
        <div className="email">{u.email}</div>
      </div>
      <button disabled={busy} onClick={async () => {
        setBusy(true)
        try {
          if (following) await unfollowUser(u.id); else await followUser(u.id)
          setFollowing(!following)
        } finally {
          setBusy(false)
        }
      }}>
        {following ? 'Following' : 'Follow'}
      </button>
    </div>
  )
}
