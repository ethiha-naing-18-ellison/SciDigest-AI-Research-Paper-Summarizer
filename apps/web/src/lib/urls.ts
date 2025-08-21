const base = process.env.NEXT_PUBLIC_API_BASE || "http://localhost:5108";

// Debug logging
if (typeof window !== 'undefined') {
  console.log("Environment variable NEXT_PUBLIC_API_BASE:", process.env.NEXT_PUBLIC_API_BASE);
  console.log("Using API base URL:", base);
}

export const urls = {
  upload: () => `${base}/api/papers`,
  paper: (id: string) => `${base}/api/papers/${id}`,
  reprocess: (id: string) => `${base}/api/papers/${id}/process`,
  export: (id: string, format: "md" | "pdf" | "pptx") => `${base}/api/papers/${id}/export?format=${format}`,
};
