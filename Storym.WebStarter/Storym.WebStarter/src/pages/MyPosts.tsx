import { useQuery,useQueryClient  } from '@tanstack/react-query'
import { getMine } from '../api/diary'
import { getMyProfile } from '../api/users'
import PostCard from '../components/PostCard'
import Avatar from '../components/Avatar'
import { abs } from '../lib/absUrl'
import { uploadAvatar } from '../api/users'
import UsersModal from '../components/UsersModal'
import { useState } from 'react'

export default function MyPosts() {
  // fetch my profile + my posts
  const profileQ = useQuery({ queryKey: ['me-profile'], queryFn: getMyProfile })
  const postsQ   = useQuery({ queryKey: ['me-posts'],   queryFn: getMine })
  const [modalOpen, setModalOpen] = useState<false | 'followers' | 'following'>(false)
  
  if (profileQ.isLoading) return <div className="card">Loading profile…</div>
  if (profileQ.error)     return <div className="card">Couldn’t load profile</div>

  const me = profileQ.data!
  const avatarAbs = me.avatarUrl ? abs(me.avatarUrl) : undefined

  return (
    <>
      {/* PROFILE HEADER */}
      <section
        className="card"
        style={{
          display: 'grid',
          gridTemplateColumns: '88px 1fr auto',
          gap: 16,
          alignItems: 'center'
        }}
      >
        {/* Avatar (click triggers hidden file input — upload wired next step) */}
        <div>
          <button
            title="Change avatar"
            onClick={() => document.getElementById('avatar-input')?.click()}
            style={{ background: 'transparent', border: 'none', padding: 0, cursor: 'pointer' }}
          >
            <Avatar src={avatarAbs} size={80} disablePreview/>
          </button>
          <input
            id="avatar-input"
  type="file"
  accept="image/*"
  style={{ display: 'none' }}
  onChange={async (e) => {
    const file = e.target.files?.[0]
    if (!file) return
    try {
      // upload to API
      // (we don’t need the return value; we’ll refetch profile)
      await uploadAvatar(file)
      // refresh header with the new avatar
      await profileQ.refetch()
    } catch (err: any) {
      alert('Avatar upload failed')
      console.error(err)
    } finally {
      e.currentTarget.value = '' // allow re-uploading same file if needed
    }
            }}
          />
        </div>

        {/* Basic info + counts */}
        <div>
          <div className="nick" style={{ fontSize: 22 }}>{me.nick}</div>
          <div className="email">{me.email}</div>

          <div style={{ marginTop: 10, display: 'flex', gap: 16, alignItems: 'center' }}>
  <span title="Total entries">{me.entryCount} entries</span>

  <button onClick={() => setModalOpen('followers')}
    style={{ background: 'transparent', border: '1px solid var(--border)', padding: '6px 10px', borderRadius: 8 }}>
    {me.followerCount} followers
  </button>

  <button onClick={() => setModalOpen('following')}
    style={{ background: 'transparent', border: '1px solid var(--border)', padding: '6px 10px', borderRadius: 8 }}>
    {me.followingCount} following
  </button>
</div>

{modalOpen && (
  <UsersModal
    open={!!modalOpen}
    onClose={() => setModalOpen(false)}
    userId={me.id}
    kind={modalOpen}
  />
)}
        </div>

        {/* Edit profile (we’ll wire an edit dialog later) */}
        <div>
          <button onClick={() => { /* TODO: open edit profile dialog */ }}>
            Edit profile
          </button>
        </div>
      </section>

      {/* MY POSTS LIST (same look as feed) */}
      {postsQ.isLoading && <div className="card">Loading posts…</div>}
      {postsQ.error && <div className="card">Couldn’t load posts</div>}
      {!postsQ.isLoading && !postsQ.error && (
        <>
          {postsQ.data!.map(e => (
  <PostCard
    key={e.id}
    entry={e}
    currentUserId={me.id}                 // ✅ pass your user id
    onChanged={() => { void postsQ.refetch() }}  // ✅ return void
  />
))}
        </>
      )}
    </>
  )
}
