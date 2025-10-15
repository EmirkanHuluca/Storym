import { useQuery } from '@tanstack/react-query'
import { getFeed } from '../api/diary'
import PostCard from '../components/PostCard'
import { getMyProfile } from '../api/users'

export default function Feed() {
  const feedQ = useQuery({ queryKey: ['feed'],       queryFn: getFeed })
  const meQ   = useQuery({   queryKey: ['me-profile'], queryFn: getMyProfile, staleTime: 60_000 })

  if (feedQ.isLoading) return <div className="card">Loading feed…</div>
  if (feedQ.error)     return <div className="card">Error loading feed</div>

  return (
    <>
      {feedQ.data!.map(e => (
        <PostCard
          key={e.id}
          entry={e}
          currentUserId={meQ.data?.id??""}            // <-- pass once
          onChanged={() => feedQ.refetch()}
        />
      ))}
    </>
  )
}
