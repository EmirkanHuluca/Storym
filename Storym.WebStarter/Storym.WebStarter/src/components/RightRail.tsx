import { useEffect, useState } from 'react'
import { searchUsers, getSuggestedUsers, followUser, unfollowUser, type UserMini } from '../api/users'
import { useQueryClient, useQuery } from '@tanstack/react-query'
import Avatar from './Avatar'
import { Link } from 'react-router-dom'

export default function RightRail() {
  const [q, setQ] = useState('')
  const [results, setResults] = useState<UserMini[]>([])
  const take = 5

  // ✅ use React Query for suggested
  const suggestedQ = useQuery({
    queryKey: ['suggested', take],
    queryFn: () => getSuggestedUsers(take),
  })

  useEffect(() => {
    const id = setTimeout(async () => {
      if (!q) { setResults([]); return }
      setResults(await searchUsers(q))
    }, 250)
    return () => clearTimeout(id)
  }, [q])

  const suggested = suggestedQ.data ?? []

  return (
    <div>
      <div className="search-box">
        <input placeholder="Search users..." value={q} onChange={e=>setQ(e.target.value)} />
      </div>

      {!!results.length && (
        <>
          <h3 style={{ marginTop: 12, marginBottom: 8 }}>Results</h3>
          <div className="suggested">
            {results.map(u => <UserRow key={u.id} user={u} />)}
          </div>
        </>
      )}

      <h3 style={{ marginTop: 16, marginBottom: 8 }}>Suggested Accounts</h3>
      <div className="suggested">
        {suggested.map(u => <UserRow key={u.id} user={u} />)}
      </div>
    </div>
  )
}

function UserRow({ user }: { user: UserMini }) {
  const [following, setFollowing] = useState<boolean>(user.following ?? false)
  const [loading, setLoading] = useState(false)
  const queryClient = useQueryClient()

useEffect(() => {
    setFollowing(user.following ?? false)
  }, [user.following, user.id])

const qc = useQueryClient()
  return (
    <div className="suggested-item">
      <Link to={`/u/${user.id}`}><Avatar src={user.avatarUrl || undefined} /></Link>
      <div>
        <Link to={`/u/${user.id}`} className="nick">{user.nick}</Link>
        <div className="email">{user.email}</div>
      </div>
      <button
        disabled={loading}
        onClick={async () => {
          setLoading(true)
          const next = !following
          try {
            // optimistic flip
            setFollowing(next)

            // optimistically patch all suggested lists in cache
            queryClient.setQueriesData({ queryKey: ['suggested'] }, (old: unknown) => {
              if (!Array.isArray(old)) return old
              return old.map((u: any) => u.id === user.id ? { ...u, following: next } : u)
            })

            if (next) await followUser(user.id)
            else      await unfollowUser(user.id)
          } finally {
            setLoading(false)
            // ensure truth from server and update every visible list
            await queryClient.invalidateQueries({ queryKey: ['suggested'] })
            await qc.invalidateQueries({ queryKey: ['profile', user.id] })
            await qc.invalidateQueries({ queryKey: ['me-profile'] })
            await queryClient.refetchQueries({ queryKey: ['suggested'], type: 'active' })
          }
        }}
      >
        {following ? 'Following' : 'Follow'}
      </button>
    </div>
  )
}
