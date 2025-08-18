"use client";

import { useState, DragEvent } from "react";
import { uploadPdf } from "@/lib/api";
import { useRouter } from "next/navigation";
import Spinner from "./Spinner";
import { Toast } from "./Toast";

export default function UploadDropzone() {
  const [hover, setHover] = useState(false);
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const router = useRouter();

  async function onFiles(files: FileList | null) {
    if (!files || files.length === 0) return;
    const file = files[0];
    if (file.type !== "application/pdf") { setToast("Please upload a PDF file."); return; }
    setBusy(true);
    try {
      const { paperId } = await uploadPdf(file);
      router.push(`/paper/${paperId}`);
    } catch (e: any) {
      setToast(e.message || "Upload failed.");
    } finally {
      setBusy(false);
    }
  }

  function onDrop(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setHover(false);
    onFiles(e.dataTransfer.files);
  }

  return (
    <>
      {toast && <Toast message={toast} onDone={() => setToast(null)} />}
      <div
        onDragOver={(e) => { e.preventDefault(); setHover(true); }}
        onDragLeave={() => setHover(false)}
        onDrop={onDrop}
        className={`relative rounded-3xl border-2 border-dashed p-12 text-center transition-all duration-300 transform ${
          hover 
            ? "border-cyan-400 bg-cyan-500/10 scale-105 shadow-2xl" 
            : "border-white/20 bg-white/5 hover:bg-white/10"
        } ${busy ? "pointer-events-none" : "cursor-pointer"}`}
      >
        {/* Background decoration */}
        <div className="absolute inset-0 rounded-3xl bg-gradient-to-br from-blue-500/5 to-purple-500/5"></div>
        
        <div className="relative z-10">
          {/* Upload icon */}
          <div className={`mx-auto w-16 h-16 rounded-full flex items-center justify-center mb-6 transition-all duration-300 ${
            hover ? "bg-cyan-500 shadow-lg" : "bg-white/10"
          }`}>
            {busy ? (
              <Spinner />
            ) : (
              <svg 
                className={`w-8 h-8 transition-colors duration-300 ${hover ? "text-white" : "text-cyan-300"}`} 
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
              </svg>
            )}
          </div>

          <div className="space-y-2 mb-6">
            <h3 className="text-xl font-bold text-gray-100">
              {busy ? "Uploading your paper..." : hover ? "Drop your PDF here!" : "Upload a research paper"}
            </h3>
            <p className="text-gray-300">
              {busy ? "Please wait while we process your file" : "Drag & drop your PDF or click to browse"}
            </p>
            <p className="text-gray-400 text-sm">
              Supports PDF files up to 50MB
            </p>
          </div>

          {/* Upload button */}
          <label className={`inline-flex cursor-pointer items-center gap-3 rounded-2xl px-8 py-4 font-semibold transition-all duration-300 transform hover:scale-105 ${
            busy 
              ? "bg-white/20 text-gray-300 cursor-not-allowed" 
              : "bg-gradient-to-r from-cyan-500 to-blue-600 text-white hover:from-cyan-600 hover:to-blue-700 shadow-lg hover:shadow-xl"
          }`}>
            {busy ? (
              <>
                <Spinner />
                <span>Processing...</span>
              </>
            ) : (
              <>
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                <span>Choose PDF File</span>
              </>
            )}
            <input 
              type="file" 
              accept="application/pdf" 
              className="hidden" 
              onChange={(e) => onFiles(e.target.files)}
              disabled={busy}
            />
          </label>

          {/* Progress indicator when busy */}
          {busy && (
            <div className="mt-6">
              <div className="w-full bg-white/20 rounded-full h-2">
                <div className="bg-gradient-to-r from-cyan-500 to-blue-500 h-2 rounded-full animate-pulse" style={{width: '60%'}}></div>
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
