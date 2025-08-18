import { AnchorDto } from "@/types/dto";
import { fmtPages } from "@/lib/utils";

export default function ContributionsList({ bullets, anchors }: { bullets?: string[]; anchors?: AnchorDto[] }) {
  if (!bullets || bullets.length === 0) return null;
  return (
    <div className="glass rounded-2xl p-6 shadow-2xl">
      <div className="mb-4 flex items-center space-x-3">
        <div className="w-8 h-8 bg-gradient-to-br from-blue-400 to-indigo-500 rounded-lg flex items-center justify-center">
          <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h2 className="text-lg font-semibold text-gray-100">Key Contributions</h2>
      </div>
      <ul className="space-y-3">
        {bullets.map((b, i) => {
          const a = anchors?.find(x => x.bulletIndex === i);
          return (
            <li key={i} className="group relative rounded-lg border border-white/10 bg-white/5 p-4 hover:bg-white/10 transition-all duration-300 transform hover:scale-105">
              <div className="leading-7 text-gray-200">{b}</div>
              {a && (
                <div className="mt-2 text-xs text-gray-400">
                  <span className="rounded bg-white/10 border border-white/20 px-2 py-1 text-gray-300">{a.sectionName}</span>
                  <span className="ml-2 text-gray-400">{fmtPages(a.pageStart, a.pageEnd)}</span>
                </div>
              )}
            </li>
          );
        })}
      </ul>
    </div>
  );
}
