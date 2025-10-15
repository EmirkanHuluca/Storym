import { useParams } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getUserProfile } from '../api/users'
import { getByUser } from '../api/diary'
import Avatar from '../components/Avatar'
import PostCard from '../components/PostCard'
import { followUser, unfollowUser } from '../api/users'
import { useEffect, useState } from 'react'
import { useAuth } from '../auth/useAuth'
import UsersModal from '../components/UsersModal'


export default function VisitProfile() {
  const { id } = useParams()
  const qc = useQueryClient()
  const { token } = useAuth()
  const [modalOpen, setModalOpen] = useState<false | 'followers' | 'following'>(false)

  const profileQ = useQuery({
    queryKey: ['profile', id],
    queryFn: () => getUserProfile(id!),
    enabled: !!id
  })
  const postsQ = useQuery({
    queryKey: ['posts', id],
    queryFn: () => getByUser(id!),
    enabled: !!id
  })

  // ✅ hooks are declared before any return
  const [following, setFollowing] = useState(false)

  // when profile data arrives, sync following flag
  useEffect(() => {
    if (profileQ.data) setFollowing(profileQ.data.following)
  }, [profileQ.data])

  // early returns AFTER hooks are declared
  if (profileQ.isLoading) return <div className="card">Loading profile…</div>
  if (profileQ.error || !profileQ.data) return <div className="card">Profile not found</div>

  const p = profileQ.data

  async function toggleFollow() {
    if (!token) return
    if (following) await unfollowUser(p.id); else await followUser(p.id)
    setFollowing(f => !f)
    qc.invalidateQueries({ queryKey: ['profile', id] })
    await qc.invalidateQueries({ queryKey: ['suggested'] })
    await qc.refetchQueries({ queryKey: ['suggested'], type: 'active' })
  }

  return (
    <>
      <section
        className="card"
        style={{ display: 'grid', gridTemplateColumns: '88px 1fr auto', gap: 16, alignItems: 'center' }}
      >
        <Avatar src={p.avatarUrl ?? undefined} size={80} />
        <div>
          <div className="nick" style={{ fontSize: 22 }}>{p.nick}</div>
          <div className="email">{p.email}</div>
          <div style={{ marginTop: 10, display: 'flex', gap: 16 }}>
            <span>{p.entryCount} entries</span>
            <div style={{ marginTop: 10, display: 'flex', gap: 16 }}>
  <button onClick={() => setModalOpen('followers')}
    style={{ background:'transparent', border:'1px solid var(--border)', padding:'6px 10px', borderRadius:8 }}>
    {p.followerCount} followers
  </button>
  <button onClick={() => setModalOpen('following')}
    style={{ background:'transparent', border:'1px solid var(--border)', padding:'6px 10px', borderRadius:8 }}>
    {p.followingCount} following
  </button>
</div>

{modalOpen && (
  <UsersModal
    open={!!modalOpen}
    onClose={() => setModalOpen(false)}
    userId={p.id}
    kind={modalOpen}
  />
)}
          </div>
        </div>
        <div>
          {p.isMe ? (
            <a href="/me"><button>Edit profile</button></a>
          ) : (
            <button disabled={!token} onClick={toggleFollow}>
              {following ? 'Following' : 'Follow'}
            </button>
          )}
        </div>
      </section>

      {postsQ.isLoading && <div className="card">Loading posts…</div>}
      {postsQ.error && <div className="card">Couldn’t load posts</div>}
      {!postsQ.isLoading && !postsQ.error && postsQ.data!.map(e =>
        <PostCard key={e.id} currentUserId={p.id} entry={e} onChanged={() => postsQ.refetch()} />
      )}
    </>
  )
}
