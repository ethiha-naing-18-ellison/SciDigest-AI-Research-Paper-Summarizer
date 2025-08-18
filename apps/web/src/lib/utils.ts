export async function copyToClipboard(text: string) {
  await navigator.clipboard.writeText(text);
}

export function fmtPages(a?: number, b?: number) {
  if (a == null || b == null) return "";
  return a === b ? `p.${a}` : `pp.${a}–${b}`;
}

export function hostOf(url?: string) {
  if (!url) return "";
  try { return new URL(url).hostname; } catch { return ""; }
}

export function isPlaceholderHost(host: string) {
  return host === "example.com" || host === "example.org";
}

export function buildScholarQuery(title?: string, authors?: string, year?: number) {
  const parts = [];
  if (title) parts.push(title);
  if (authors) parts.push(authors.split(/[;,]/)[0]); // first author helps
  const q = parts.join(" ").trim();
  const params = new URLSearchParams({ q });
  if (year && Number.isFinite(year)) {
    params.set("as_ylo", String(year));
    params.set("as_yhi", String(year));
  }
  // English UI; adjust if you prefer
  params.set("hl", "en");
  return `https://scholar.google.com/scholar?${params.toString()}`;
}

export function resolveRelatedLink(
  item: { title?: string; authors?: string; year?: number; url?: string },
  mode: "scholar_fallback" | "scholar_always" = "scholar_fallback"
) {
  const scholarUrl = buildScholarQuery(item.title, item.authors, item.year);
  if (mode === "scholar_always") return scholarUrl;

  const host = hostOf(item.url);
  // If no URL, bad URL, or placeholder — use Scholar
  if (!item.url || !host || isPlaceholderHost(host)) return scholarUrl;
  return item.url;
}