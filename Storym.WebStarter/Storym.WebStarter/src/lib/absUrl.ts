export const apiBase = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');

export function abs(url: string) {
  if (!url) return url;
  if (/^https?:\/\//i.test(url)) return url; // already absolute
  if (url.startsWith('/')) return `${apiBase}${url}`;
  return `${apiBase}/${url}`;
}

export function rel(u: string): string {
  if (!u) return u
  // if it already looks relative, keep it
  if (u.startsWith('/')) return u
  try {
    const url = new URL(u)
    // same-origin absolute -> return the path (and keep the path only)
    if (apiBase && url.origin === new URL(apiBase).origin) return url.pathname
    // generic absolute -> keep just the path (most CDNs still serve /uploads/*)
    return url.pathname
  } catch {
    // not a valid absolute, fall back to original
    return u
  }
}