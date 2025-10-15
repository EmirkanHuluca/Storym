import React, { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { createPortal } from 'react-dom'

type Ctx = {
  open: (images: string[] | string, startIndex?: number) => void
  close: () => void
}

const ViewerCtx = createContext<Ctx>({ open: () => {}, close: () => {} })

export function ImageViewerProvider({ children }: { children: React.ReactNode }) {
  const [open, setOpen] = useState(false)
  const [images, setImages] = useState<string[]>([])
  const [index, setIndex] = useState(0)

  const api = useMemo<Ctx>(() => ({
    open: (imgs, start = 0) => {
      const arr = Array.isArray(imgs) ? imgs : [imgs]
      if (arr.length === 0) return
      setImages(arr)
      setIndex(Math.max(0, Math.min(start, arr.length - 1)))
      setOpen(true)
    },
    close: () => setOpen(false)
  }), [])

  useEffect(() => {
    if (!open) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false)
      if (e.key === 'ArrowRight') setIndex(i => Math.min(i + 1, images.length - 1))
      if (e.key === 'ArrowLeft') setIndex(i => Math.max(i - 1, 0))
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [open, images.length])

  return (
    <ViewerCtx.Provider value={api}>
      {children}
      {open && createPortal(
        <div className="viewer-overlay" onClick={() => setOpen(false)}>
          <div className="viewer-inner" onClick={e => e.stopPropagation()}>
            {images.length > 1 && (
              <button className="viewer-nav left" onClick={() => setIndex(i => Math.max(i - 1, 0))}>&lsaquo;</button>
            )}
            <img className="viewer-img" src={images[index]} alt="" />
            {images.length > 1 && (
              <button className="viewer-nav right" onClick={() => setIndex(i => Math.min(i + 1, images.length - 1))}>&rsaquo;</button>
            )}
            <button className="viewer-close" onClick={() => setOpen(false)}>×</button>
          </div>
        </div>,
        document.body
      )}
    </ViewerCtx.Provider>
  )
}

export function useImageViewer() {
  return useContext(ViewerCtx)
}
