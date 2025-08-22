"use client";

import { useState, DragEvent } from "react";
import { uploadPdf } from "@/lib/api";
import { useRouter } from "next/navigation";
import Spinner from "./Spinner";
import { Toast } from "./Toast";

interface UploadProgress {
  file: File;
  status: 'uploading' | 'completed' | 'failed';
  paperId?: string;
  error?: string;
  progress: number;
}

export default function BatchUploadDropzone() {
  const [hover, setHover] = useState(false);
  const [uploads, setUploads] = useState<UploadProgress[]>([]);
  const [toast, setToast] = useState<string | null>(null);
  const router = useRouter();

  async function processFile(file: File, index: number) {
    try {
      // Update status to uploading
      setUploads(prev => prev.map((upload, i) => 
        i === index ? { ...upload, status: 'uploading', progress: 20 } : upload
      ));

      // Simulate progress updates
      const progressInterval = setInterval(() => {
        setUploads(prev => prev.map((upload, i) => 
          i === index && upload.status === 'uploading' 
            ? { ...upload, progress: Math.min(upload.progress + 10, 90) } 
            : upload
        ));
      }, 500);

      const { paperId } = await uploadPdf(file);
      
      clearInterval(progressInterval);
      
      // Update to completed
      setUploads(prev => prev.map((upload, i) => 
        i === index ? { 
          ...upload, 
          status: 'completed', 
          progress: 100, 
          paperId 
        } : upload
      ));

    } catch (e: any) {
      // Update to failed
      setUploads(prev => prev.map((upload, i) => 
        i === index ? { 
          ...upload, 
          status: 'failed', 
          progress: 0,
          error: e.message || "Upload failed" 
        } : upload
      ));
    }
  }

  async function onFiles(files: FileList | null) {
    if (!files || files.length === 0) return;
    
    const supportedTypes = [
      "application/pdf",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "application/msword",
      "text/html",
      "application/xhtml+xml",
      "text/x-tex",
      "application/x-tex",
      "text/plain"
    ];
    
    const supportedFiles = Array.from(files).filter(file => supportedTypes.includes(file.type));
    
    if (supportedFiles.length === 0) {
      setToast("Please upload supported files (PDF, DOCX, DOC, HTML, TEX, TXT) only.");
      return;
    }

    if (supportedFiles.length > 10) {
      setToast("Maximum 10 files allowed per batch.");
      return;
    }

    // Initialize upload progress for all files
    const newUploads = supportedFiles.map(file => ({
      file,
      status: 'uploading' as const,
      progress: 0
    }));

    setUploads(newUploads);

    // Process files sequentially to avoid overwhelming the server
    for (let i = 0; i < supportedFiles.length; i++) {
      await processFile(supportedFiles[i], i);
      // Small delay between uploads
      if (i < supportedFiles.length - 1) {
        await new Promise(resolve => setTimeout(resolve, 1000));
      }
    }
  }

  function onDrop(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setHover(false);
    onFiles(e.dataTransfer.files);
  }

  function clearUploads() {
    setUploads([]);
  }

  function viewPaper(paperId: string) {
    router.push(`/paper/${paperId}`);
  }

  const isUploading = uploads.some(upload => upload.status === 'uploading');
  const completedCount = uploads.filter(upload => upload.status === 'completed').length;
  const failedCount = uploads.filter(upload => upload.status === 'failed').length;

  return (
    <>
      {toast && <Toast message={toast} onDone={() => setToast(null)} />}
      
      <div className="space-y-6">
        {/* Main Upload Area */}
        <div
          className={`glass rounded-3xl p-8 text-center transition-all duration-300 border-2 border-dashed ${
            hover 
              ? "border-cyan-400 dark:bg-cyan-500/10 bg-cyan-400/10 scale-105" 
              : "border-white/20 hover:border-white/40"
          } ${isUploading ? "pointer-events-none opacity-75" : "cursor-pointer hover:scale-105"}`}
          onDrop={onDrop}
          onDragOver={(e) => e.preventDefault()}
          onDragEnter={() => setHover(true)}
          onDragLeave={() => setHover(false)}
        >
          {/* Upload Icon */}
          <div className="mx-auto mb-6 w-20 h-20 bg-gradient-to-br from-cyan-400 to-blue-600 rounded-2xl flex items-center justify-center shadow-lg">
            {isUploading ? (
              <Spinner />
            ) : (
              <svg className="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M9 11l3-3m0 0l3 3m-3-3v8" />
              </svg>
            )}
          </div>

          <div className="space-y-2 mb-6">
            <h3 className="text-xl font-bold dark:text-gray-100 text-gray-900">
              {isUploading ? "Processing your papers..." : hover ? "Drop your PDFs here!" : "Batch Upload Research Papers"}
            </h3>
            <p className="dark:text-gray-300 text-gray-700">
              {isUploading ? "Please wait while we process your files" : "Drag & drop multiple files or click to browse"}
            </p>
            <p className="dark:text-gray-400 text-gray-500 text-sm">
              Supports up to 10 files (PDF, DOCX, DOC, HTML, TEX, TXT), max 50MB each
            </p>
          </div>

          {/* Upload button */}
          <label className={`inline-flex cursor-pointer items-center gap-3 rounded-2xl px-8 py-4 font-semibold transition-all duration-300 transform hover:scale-105 ${
            isUploading 
              ? "bg-white/20 dark:text-gray-300 text-gray-700 cursor-not-allowed" 
              : "bg-gradient-to-r from-cyan-500 to-blue-600 text-white hover:from-cyan-600 hover:to-blue-700 shadow-lg hover:shadow-xl"
          }`}>
            {isUploading ? (
              <>
                <Spinner />
                <span>Processing...</span>
              </>
            ) : (
              <>
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                <span>Choose Files</span>
              </>
            )}
            <input 
              type="file" 
              accept=".pdf,.docx,.doc,.html,.htm,.xhtml,.tex,.txt" 
              multiple
              className="hidden" 
              onChange={(e) => onFiles(e.target.files)}
              disabled={isUploading}
            />
          </label>
        </div>

        {/* Upload Progress List */}
        {uploads.length > 0 && (
          <div className="glass rounded-2xl p-6">
            <div className="flex items-center justify-between mb-4">
              <h4 className="text-lg font-semibold dark:text-gray-100 text-gray-900">
                Upload Progress ({completedCount}/{uploads.length})
              </h4>
              {!isUploading && (
                <button
                  onClick={clearUploads}
                  className="text-sm dark:text-gray-400 text-gray-700 hover:dark:text-gray-200 hover:text-gray-800 transition-colors"
                >
                  Clear All
                </button>
              )}
            </div>

            <div className="space-y-3">
              {uploads.map((upload, index) => (
                <div key={index} className="flex items-center gap-4 p-3 dark:bg-white/5 bg-black/5 rounded-xl">
                  {/* Status Icon */}
                  <div className={`w-8 h-8 rounded-full flex items-center justify-center ${
                    upload.status === 'completed' ? 'bg-green-500' :
                    upload.status === 'failed' ? 'bg-red-500' :
                    'bg-blue-500'
                  }`}>
                    {upload.status === 'uploading' ? (
                      <Spinner />
                    ) : upload.status === 'completed' ? (
                      <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                      </svg>
                    ) : (
                      <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    )}
                  </div>

                  {/* File Info */}
                  <div className="flex-1 min-w-0">
                    <p className="font-medium dark:text-gray-200 text-gray-800 truncate">
                      {upload.file.name}
                    </p>
                    <p className="text-sm dark:text-gray-400 text-gray-700">
                      {(upload.file.size / 1024 / 1024).toFixed(1)} MB
                    </p>
                    {upload.status === 'uploading' && (
                      <div className="mt-2 w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                        <div 
                          className="bg-blue-500 h-2 rounded-full transition-all duration-300"
                          style={{ width: `${upload.progress}%` }}
                        ></div>
                      </div>
                    )}
                    {upload.error && (
                      <p className="text-sm text-red-400 mt-1">{upload.error}</p>
                    )}
                  </div>

                  {/* Action Button */}
                  {upload.status === 'completed' && upload.paperId && (
                    <button
                      onClick={() => viewPaper(upload.paperId!)}
                      className="px-4 py-2 bg-gradient-to-r from-green-500 to-emerald-600 text-white text-sm rounded-lg hover:from-green-600 hover:to-emerald-700 transition-all duration-300"
                    >
                      View Summary
                    </button>
                  )}
                </div>
              ))}
            </div>

            {/* Summary Stats */}
            {uploads.length > 0 && (
              <div className="mt-4 p-3 dark:bg-white/5 bg-black/5 rounded-lg">
                <div className="flex items-center justify-between text-sm">
                  <span className="dark:text-gray-400 text-gray-700">
                    {completedCount} completed • {failedCount} failed • {uploads.length - completedCount - failedCount} processing
                  </span>
                  {completedCount > 0 && (
                    <button
                      onClick={() => {
                        // Navigate to the first completed paper or show a batch view
                        const firstCompleted = uploads.find(u => u.status === 'completed');
                        if (firstCompleted?.paperId) {
                          viewPaper(firstCompleted.paperId);
                        }
                      }}
                      className="text-cyan-400 hover:text-cyan-300 font-medium"
                    >
                      View Results →
                    </button>
                  )}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </>
  );
}
