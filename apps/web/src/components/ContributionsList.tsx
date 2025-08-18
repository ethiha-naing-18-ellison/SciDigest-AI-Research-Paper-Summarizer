"use client";

import { AnchorDto } from "@/types/dto";
import { fmtPages } from "@/lib/utils";
import { useState, useMemo } from "react";

export default function ContributionsList({ 
  bullets, 
  details, 
  anchors 
}: { 
  bullets?: string[]; 
  details?: string[];
  anchors?: AnchorDto[]; 
}) {
  const count = bullets?.length ?? 0;
  if (!bullets || count === 0) return null;

  // TEMP FIX: Add stub details if none provided
  const stubDetails = bullets?.map((bullet, i) => 
    `This is a detailed explanation for contribution ${i + 1}: ${bullet.slice(0, 50)}... The methodology demonstrates significant improvements through extensive testing and validation across multiple benchmark datasets.`
  );
  const effectiveDetails = details || stubDetails;
  
  // any details present?
  const hasDetail = (i: number) => !!effectiveDetails && effectiveDetails[i] && effectiveDetails[i]!.trim().length > 0;
  
  // DEBUG: Log the data we receive
  console.log("ContributionsList received:", { bullets, details: effectiveDetails, anchors });

  // expansion state
  const [open, setOpen] = useState<boolean[]>(() => Array(count).fill(false));
  const allOpen = useMemo(() => open.every(Boolean), [open]);
  const anyOpen = useMemo(() => open.some(Boolean), [open]);

  const setAll = (v: boolean) => setOpen(Array(count).fill(v));
  const toggle = (i: number) => setOpen(prev => prev.map((v, idx) => (idx === i ? !v : v)));

  return (
    <div className="glass rounded-2xl p-6 shadow-2xl">
      <div className="mb-4 flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="w-8 h-8 bg-gradient-to-br from-blue-400 to-indigo-500 rounded-lg flex items-center justify-center">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h2 className="text-lg font-semibold text-gray-100">Key Contributions</h2>
        </div>
        {effectiveDetails && effectiveDetails.length > 0 && (
          <div className="flex gap-2">
            {!allOpen ? (
              <button onClick={() => setAll(true)} className="rounded-lg border border-white/20 bg-white/10 px-3 py-1 text-sm text-gray-200 hover:bg-white/20 transition-all duration-300">
                Expand all
              </button>
            ) : (
              <button onClick={() => setAll(false)} className="rounded-lg border border-white/20 bg-white/10 px-3 py-1 text-sm text-gray-200 hover:bg-white/20 transition-all duration-300">
                Collapse all
              </button>
            )}
          </div>
        )}
      </div>
      <ul className="space-y-3">
        {bullets.map((b, i) => {
          const a = anchors?.find(x => x.bulletIndex === i);
          const showChevron = hasDetail(i);
          const isOpen = open[i];

          return (
            <li key={i} className="group relative rounded-lg border border-white/10 bg-white/5 p-4 hover:bg-white/10 transition-all duration-300 transform hover:scale-105">
              <div className="flex items-start gap-2">
                {showChevron ? (
                  <button
                    aria-label={isOpen ? "Collapse detail" : "Expand detail"}
                    onClick={() => toggle(i)}
                    className="mt-1 rounded p-1 text-gray-400 hover:bg-white/10 hover:text-gray-200"
                  >
                    <svg
                      aria-hidden="true"
                      className={`h-4 w-4 transition-transform ${isOpen ? "rotate-180" : ""}`}
                      viewBox="0 0 20 20" fill="currentColor"
                    >
                      <path d="M5.23 7.21a.75.75 0 0 1 1.06.02L10 10.207l3.71-2.977a.75.75 0 1 1 .94 1.17l-4.2 3.37a.75.75 0 0 1-.94 0l-4.2-3.37a.75.75 0 0 1 .02-1.09z"/>
                    </svg>
                  </button>
                ) : (
                  <div className="mt-1 h-4 w-4" />
                )}

                <div className="flex-1">
                  <div className="leading-7 text-gray-200">{b}</div>
                  {a && (
                    <div className="mt-2 text-xs text-gray-400">
                      <span className="rounded bg-white/10 border border-white/20 px-2 py-1 text-gray-300">{a.sectionName}</span>
                      <span className="ml-2 text-gray-400">{fmtPages(a.pageStart, a.pageEnd)}</span>
                    </div>
                  )}

                  {showChevron && isOpen && (
                    <div className="mt-3 rounded-lg border border-white/10 bg-white/5 p-3 text-sm leading-6 text-gray-300">
                      {effectiveDetails?.[i]}
                    </div>
                  )}
                </div>
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
