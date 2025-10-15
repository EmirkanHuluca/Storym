export type DiaryEntryDto = {
  id: number
  title: string
  summary: string
  content: string
  date: string
  isHidden: boolean
  userId: string
  imageUrls: string[]
  likeCount: number
  likedByMe: boolean
  ownerNick: string
  ownerEmail: string
  ownerAvatarUrl?: string | null
  ownerFollowing?: boolean
}
