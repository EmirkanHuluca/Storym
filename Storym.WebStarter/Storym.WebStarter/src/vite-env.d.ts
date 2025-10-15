/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** e.g. https://localhost:7164 */
  readonly VITE_API_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
