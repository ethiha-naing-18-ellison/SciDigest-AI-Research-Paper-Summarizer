"use client";

export default function DebugEnv() {
  return (
    <div className="glass rounded-xl p-4 border border-amber-500/30">
      <div className="flex items-center space-x-2 mb-2">
        <div className="w-6 h-6 bg-amber-500 rounded-lg flex items-center justify-center">
          <svg className="w-4 h-4 text-amber-900" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <strong className="text-amber-300 font-semibold">Debug Info</strong>
      </div>
      <div className="space-y-1 text-xs text-gray-300">
        <div><span className="text-amber-200">API Base:</span> {process.env.NEXT_PUBLIC_API_BASE || "UNDEFINED"}</div>
        <div><span className="text-amber-200">Environment:</span> {process.env.NODE_ENV}</div>
      </div>
    </div>
  );
}
