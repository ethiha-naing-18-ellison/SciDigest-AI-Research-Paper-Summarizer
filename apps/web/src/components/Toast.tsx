"use client";
import { useEffect, useState } from "react";

export function Toast({ message, onDone }: { message: string; onDone?: () => void }) {
  const [open, setOpen] = useState(true);
  useEffect(() => {
    const t = setTimeout(() => { setOpen(false); onDone?.(); }, 3500);
    return () => clearTimeout(t);
  }, [onDone]);
  
  if (!open) return null;
  
  return (
    <div className="fixed right-4 top-4 z-50 animate-in slide-in-from-right-full duration-300">
      <div className="glass rounded-xl px-6 py-4 shadow-2xl backdrop-blur-md border border-white/20">
        <div className="flex items-center space-x-3">
          <div className="w-2 h-2 bg-blue-400 rounded-full animate-pulse"></div>
          <span className="text-white font-medium">{message}</span>
        </div>
      </div>
    </div>
  );
}
