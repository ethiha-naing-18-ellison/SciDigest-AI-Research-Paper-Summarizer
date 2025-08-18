export async function copyToClipboard(text: string) {
  await navigator.clipboard.writeText(text);
}

export function fmtPages(a?: number, b?: number) {
  if (a == null || b == null) return "";
  return a === b ? `p.${a}` : `pp.${a}–${b}`;
}
