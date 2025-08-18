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

export async function exportFile(id: string, format: "md" | "pdf"): Promise<Blob> {
  const res = await fetch(urls.export(id, format), { method: "POST" });
  if (!res.ok) throw new Error(await safeText(res));
  return res.blob();
}

async function safeText(res: Response) {
  try { return await res.text(); } catch { return `${res.status} ${res.statusText}`; }
}
