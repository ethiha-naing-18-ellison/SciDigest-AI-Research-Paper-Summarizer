import type { RelatedPayload, RelatedItem } from "@/types/dto";
import { resolveRelatedLink, hostOf, isPlaceholderHost, buildScholarQuery } from "@/lib/utils";

const MODE = (process.env.NEXT_PUBLIC_RELATED_LINK_MODE as "scholar_fallback" | "scholar_always") || "scholar_fallback";

export default function RelatedGrid({ data }: { data?: RelatedPayload }) {
  if (!data || !data.items?.length) return null;

  const getLinkOptions = (item: RelatedItem) => {
    const options = [];
    
    // Primary URL (direct paper link)
    if (item.url && !isPlaceholderHost(hostOf(item.url))) {
      const host = hostOf(item.url);
      if (host.includes('arxiv.org')) {
        options.push({ label: 'arXiv', url: item.url, primary: true });
      } else if (host.includes('semanticscholar.org')) {
        options.push({ label: 'Semantic Scholar', url: item.url, primary: true });
      } else if (host.includes('openalex.org')) {
        options.push({ label: 'OpenAlex', url: item.url, primary: true });
      } else {
        options.push({ label: host || 'Direct Link', url: item.url, primary: true });
      }
    }
    
    // Alternative URLs from the item
    if ((item as any).alternative_urls) {
      (item as any).alternative_urls.forEach((url: string) => {
        const host = hostOf(url);
        if (host.includes('arxiv.org')) {
          options.push({ label: 'arXiv', url });
        } else if (host.includes('semanticscholar.org')) {
          options.push({ label: 'Semantic Scholar', url });
        } else if (host.includes('openalex.org')) {
          options.push({ label: 'OpenAlex', url });
        }
      });
    }
    
    // Google Scholar as fallback
    const scholarUrl = buildScholarQuery(item.title, item.authors, item.year);
    options.push({ label: 'Google Scholar', url: scholarUrl });
    
    return options;
  };

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
        <div className="text-xs text-gray-400 bg-white/10 border border-white/20 rounded-full px-2 py-1">
          Direct paper links available
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {data.items.map((it, idx) => {
          const linkOptions = getLinkOptions(it);
          const primaryLink = linkOptions.find(opt => opt.primary) || linkOptions[0];
          
          const content = (
            <>
              <div className="flex items-center gap-2">
                <div className="font-medium text-gray-100">{it.title}</div>
                {(it as any).source && (
                  <span className="rounded bg-white/10 border border-white/20 px-2 py-0.5 text-xs text-gray-300">{(it as any).source}</span>
                )}
              </div>
              <div className="text-sm text-gray-300 mb-2">
                {it.authors}{it.venue ? ` • ${it.venue}` : ""}{it.year ? ` • ${it.year}` : ""}
              </div>
              {it.reason && <div className="text-sm text-gray-400 border-l-2 border-purple-400/50 pl-3 mb-2">{it.reason}</div>}
              
              {/* Link Options */}
              <div className="flex flex-wrap gap-2 mt-3">
                {linkOptions.map((option, linkIdx) => (
                  <a
                    key={linkIdx}
                    href={option.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className={`text-xs px-2 py-1 rounded border transition-colors ${
                      option.primary 
                        ? 'bg-purple-500/20 border-purple-400/50 text-purple-300 hover:bg-purple-500/30' 
                        : 'bg-white/5 border-white/20 text-gray-300 hover:bg-white/10'
                    }`}
                    onClick={(e) => e.stopPropagation()}
                  >
                    {option.label}
                  </a>
                ))}
              </div>
              
              {/* Primary link indicator */}
              {primaryLink && primaryLink.primary && (
                <div className="text-xs text-green-400 mt-2 flex items-center gap-1">
                  <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                  </svg>
                  Direct paper link available
                </div>
              )}
            </>
          );

          return (
            <div
              key={idx}
              className="rounded-xl border border-white/10 bg-white/5 p-4 hover:bg-white/10 transition-all duration-300 transform hover:scale-105"
            >
              {content}
            </div>
          );
        })}
      </div>
    </div>
  );
}
