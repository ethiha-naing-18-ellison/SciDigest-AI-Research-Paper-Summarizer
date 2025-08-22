import type { PaperDto } from "@/types/dto";
import { urls } from "./urls";

export async function uploadPdf(file: File): Promise<{ paperId: string }> {
  const uploadUrl = urls.upload();
  console.log("Uploading to:", uploadUrl);
  const form = new FormData();
  form.append("file", file);
  const res = await fetch(uploadUrl, { method: "POST", body: form });
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function getPaper(id: string): Promise<PaperDto> {
  const res = await fetch(urls.paper(id), { cache: "no-store" });
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function reprocess(id: string): Promise<void> {
  const res = await fetch(urls.reprocess(id), { method: "POST" });
  if (!res.ok) throw new Error(await safeText(res));
}

export async function exportFile(id: string, format: "md" | "pdf" | "pptx"): Promise<Blob> {
  const res = await fetch(urls.export(id, format), { method: "POST" });
  if (!res.ok) throw new Error(await safeText(res));
  return res.blob();
}

export interface SearchParams {
  searchTerm?: string;
  venue?: string;
  year?: number;
  status?: string;
  sortBy?: string;
  sortDescending?: boolean;
  skip?: number;
  take?: number;
}

export interface SearchResponse {
  papers: PaperDto[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
}

export interface FilterOptions {
  venues: string[];
  years: number[];
  statuses: string[];
}

export async function searchPapers(params: SearchParams): Promise<SearchResponse> {
  const url = new URL(urls.searchPapers());
  
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      url.searchParams.append(key, value.toString());
    }
  });

  const res = await fetch(url.toString());
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function getFilterOptions(): Promise<FilterOptions> {
  const res = await fetch(urls.filterOptions());
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

// Reading Lists API
export interface ReadingList {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
  items: ReadingListItem[];
}

export interface ReadingListItem {
  id: string;
  paperId: string;
  notes?: string;
  addedAt: string;
  orderIndex: number;
  paper?: PaperDto;
}

export interface CreateReadingListRequest {
  name: string;
  description?: string;
}

export interface UpdateReadingListRequest {
  name: string;
  description?: string;
}

export interface AddPaperToListRequest {
  paperId: string;
  notes?: string;
}

export async function getReadingLists(): Promise<ReadingList[]> {
  const res = await fetch(urls.readingLists());
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function getReadingList(id: string): Promise<ReadingList> {
  const res = await fetch(urls.readingList(id));
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function createReadingList(request: CreateReadingListRequest): Promise<ReadingList> {
  const res = await fetch(urls.readingLists(), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function updateReadingList(id: string, request: UpdateReadingListRequest): Promise<ReadingList> {
  const res = await fetch(urls.readingList(id), {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function deleteReadingList(id: string): Promise<void> {
  const res = await fetch(urls.readingList(id), { method: 'DELETE' });
  if (!res.ok) throw new Error(await safeText(res));
}

export async function addPaperToList(listId: string, request: AddPaperToListRequest): Promise<ReadingListItem> {
  const res = await fetch(urls.readingListPapers(listId), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!res.ok) throw new Error(await safeText(res));
  return res.json();
}

export async function removePaperFromList(listId: string, paperId: string): Promise<void> {
  const res = await fetch(urls.readingListPaper(listId, paperId), { method: 'DELETE' });
  if (!res.ok) throw new Error(await safeText(res));
}

export async function updateItemNotes(itemId: string, notes: string): Promise<void> {
  const res = await fetch(urls.readingListItemNotes(itemId), {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ notes }),
  });
  if (!res.ok) throw new Error(await safeText(res));
}

export async function reorderItems(listId: string, itemOrders: Record<string, number>): Promise<void> {
  const res = await fetch(urls.readingListReorder(listId), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ itemOrders }),
  });
  if (!res.ok) throw new Error(await safeText(res));
}

async function safeText(res: Response) {
  try { return await res.text(); } catch { return `${res.status} ${res.statusText}`; }
}
