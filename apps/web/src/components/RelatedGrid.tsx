import type { RelatedPayload } from "@/types/dto";

export default function RelatedGrid({ data }: { data?: RelatedPayload }) {
  if (!data || !data.items?.length) return null;
  return (
    <div className="glass rounded-2xl p-6 shadow-2xl">
      <div className="mb-4 flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="w-8 h-8 bg-gradient-to-br from-purple-400 to-pink-500 rounded-lg flex items-center justify-center">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
            </svg>
          </div>
          <h2 className="text-lg font-semibold text-gray-100">Suggested Related Work</h2>
        </div>
        <div className="text-xs text-gray-400 bg-white/10 border border-white/20 rounded-full px-2 py-1">Provider: {data.provider}</div>
      </div>
      <div className="grid gap-4 md:grid-cols-2">
        {data.items.map((it, idx) => (
          <a key={idx} className="block rounded-xl border border-white/10 bg-white/5 p-4 hover:bg-white/10 transition-all duration-300 transform hover:scale-105" href={it.url} target="_blank" rel="noreferrer">
            <div className="font-medium text-gray-100 mb-2">{it.title}</div>
            <div className="text-sm text-gray-300 mb-2">{it.authors}{it.venue ? ` • ${it.venue}` : ""}{it.year ? ` • ${it.year}` : ""}</div>
            {it.reason && <div className="text-sm text-gray-400 border-l-2 border-purple-400/50 pl-3">{it.reason}</div>}
          </a>
        ))}
      </div>
    </div>
  );
}
