"use client";

import { exportFile } from "@/lib/api";
import { useState } from "react";
import Spinner from "./Spinner";
import { Toast } from "./Toast";

export default function ExportButtons({ id }: { id: string }) {
  const [busy, setBusy] = useState<"md" | "pdf" | "pptx" | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  async function doExport(fmt: "md" | "pdf" | "pptx") {
    setBusy(fmt);
    try {
      const blob = await exportFile(id, fmt);
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fmt === "md" ? "summary.md" : fmt === "pdf" ? "summary.pdf" : "presentation.html";
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
    } catch (e: any) {
      setToast(e.message || "Export failed.");
    } finally {
      setBusy(null);
    }
  }

  return (
    <>
      {toast && <Toast message={toast} onDone={() => setToast(null)} />}
      <div className="space-y-3">
        <div className="flex items-center space-x-3 mb-4">
          <div className="w-8 h-8 bg-gradient-to-br from-orange-400 to-red-500 rounded-lg flex items-center justify-center">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-100">Export Options</h3>
        </div>
        <div className="flex gap-3 flex-wrap">
          <button
            onClick={() => doExport("md")}
            className="flex items-center gap-2 rounded-xl border border-white/20 bg-white/10 px-4 py-2 text-gray-200 hover:bg-white/20 transition-all duration-300 transform hover:scale-105 disabled:opacity-50"
            disabled={busy !== null}
          >
            {busy === "md" ? <Spinner /> : (
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            )}
            <span>Export Markdown</span>
          </button>
          <button
            onClick={() => doExport("pdf")}
            className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 px-4 py-2 text-white hover:from-cyan-600 hover:to-blue-700 transition-all duration-300 transform hover:scale-105 disabled:opacity-50"
            disabled={busy !== null}
          >
            {busy === "pdf" ? <Spinner /> : (
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            )}
            <span>Export PDF</span>
          </button>
          <button
            onClick={() => doExport("pptx")}
            className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-purple-500 to-pink-600 px-4 py-2 text-white hover:from-purple-600 hover:to-pink-700 transition-all duration-300 transform hover:scale-105 disabled:opacity-50"
            disabled={busy !== null}
          >
            {busy === "pptx" ? <Spinner /> : (
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 4V2a1 1 0 011-1h8a1 1 0 011 1v2h4a1 1 0 011 1v14a1 1 0 01-1 1H3a1 1 0 01-1-1V5a1 1 0 011-1h4zM9 4v1h6V4H9z" />
              </svg>
            )}
            <span>Export Presentation</span>
          </button>
        </div>
      </div>
    </>
  );
}
