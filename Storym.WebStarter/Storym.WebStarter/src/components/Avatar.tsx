import { useImageViewer } from './ImageViewer'

export default function Avatar({
  src,
  size = 36,
  disablePreview = false,   // <-- new prop
}: { src?: string; size?: number; disablePreview?: boolean }) {
  const { open } = useImageViewer()
  const clickable = !!src && !disablePreview
  return (
    <img
      className="avatar"
      src={src || 'data:image/svg+xml;utf8,<svg xmlns=%22http://www.w3.org/2000/svg%22 width=%2236%22 height=%2236%22><rect width=%2236%22 height=%2236%22 fill=%22%23d64c4c%22/></svg>'}
      alt=""
      style={{ width: size, height: size, cursor: clickable ? 'zoom-in' : 'default' }}
      onClick={clickable ? () => open([src!], 0) : undefined}
    />
  )
}
