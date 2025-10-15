export default function Modal({ open, onClose, children, title }:{
  open: boolean; onClose: () => void; children: React.ReactNode; title?: string
}) {
  if (!open) return null
  return (
    <div className="viewer-overlay" onClick={onClose}>
      <div className="card" style={{ maxWidth: 520, width: '92vw' }} onClick={e => e.stopPropagation()}>
        <div style={{ display:'flex', justifyContent:'space-between', alignItems:'center', marginBottom: 8 }}>
          <h3 style={{ margin: 0 }}>{title}</h3>
          <button onClick={onClose}>×</button>
        </div>
        {children}
      </div>
    </div>
  )
}
