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
  searchPapers: () => `${base}/api/papers/search`,
  filterOptions: () => `${base}/api/papers/filter-options`,
  readingLists: () => `${base}/api/readinglists`,
  readingList: (id: string) => `${base}/api/readinglists/${id}`,
  readingListPapers: (listId: string) => `${base}/api/readinglists/${listId}/papers`,
  readingListPaper: (listId: string, paperId: string) => `${base}/api/readinglists/${listId}/papers/${paperId}`,
  readingListItemNotes: (itemId: string) => `${base}/api/readinglists/items/${itemId}/notes`,
  readingListReorder: (listId: string) => `${base}/api/readinglists/${listId}/reorder`,
};
