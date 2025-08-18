"use client";

import { copyToClipboard } from "@/lib/utils";
import { useState } from "react";
import { Toast } from "./Toast";

export default function SummaryCard({ text }: { text?: string }) {
  const [toast, setToast] = useState<string | null>(null);
  if (!text) return null;
  return (
    <>
      {toast && <Toast message={toast} onDone={() => setToast(null)} />}
      <div className="glass rounded-2xl p-6 shadow-2xl">
        <div className="mb-4 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-gradient-to-br from-emerald-400 to-teal-500 rounded-lg flex items-center justify-center">
              <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <h2 className="text-lg font-semibold text-gray-100">Executive Summary</h2>
          </div>
          <button
            className="rounded-lg border border-white/20 bg-white/10 px-3 py-1 text-sm text-gray-200 hover:bg-white/20 transition-all duration-300 transform hover:scale-105"
            onClick={async () => { await copyToClipboard(text); setToast("Summary copied."); }}
          >
            Copy
          </button>
        </div>
        <p className="text-gray-300 leading-7">{text}</p>
      </div>
    </>
  );
}
