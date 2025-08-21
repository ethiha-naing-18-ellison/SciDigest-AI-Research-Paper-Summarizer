"use client";

import { useState } from "react";

type UploadMode = 'single' | 'batch';

interface UploadModeToggleProps {
  mode: UploadMode;
  onModeChange: (mode: UploadMode) => void;
}

export default function UploadModeToggle({ mode, onModeChange }: UploadModeToggleProps) {
  return (
    <div className="flex items-center justify-center mb-6">
      <div className="glass rounded-2xl p-2 flex items-center gap-2">
        <button
          onClick={() => onModeChange('single')}
          className={`
            flex items-center gap-2 px-6 py-3 rounded-xl font-medium transition-all duration-300
            ${mode === 'single' 
              ? 'bg-gradient-to-r from-cyan-500 to-blue-600 text-white shadow-lg' 
              : 'dark:text-gray-400 text-gray-600 hover:dark:text-gray-200 hover:text-gray-800'
            }
          `}
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <span>Single Upload</span>
        </button>
        
        <button
          onClick={() => onModeChange('batch')}
          className={`
            flex items-center gap-2 px-6 py-3 rounded-xl font-medium transition-all duration-300
            ${mode === 'batch' 
              ? 'bg-gradient-to-r from-purple-500 to-pink-600 text-white shadow-lg' 
              : 'dark:text-gray-400 text-gray-600 hover:dark:text-gray-200 hover:text-gray-800'
            }
          `}
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          <span>Batch Upload</span>
          <span className="px-2 py-1 bg-gradient-to-r from-orange-400 to-red-500 text-xs rounded-full text-white font-bold">
            NEW
          </span>
        </button>
      </div>
    </div>
  );
}
