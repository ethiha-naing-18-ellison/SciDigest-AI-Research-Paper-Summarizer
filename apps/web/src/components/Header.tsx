"use client";

import ThemeToggle from "./ThemeToggle";

export default function Header() {
  return (
    <header className="mb-8">
      <div className="glass rounded-2xl p-6 shadow-2xl">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <div className="w-12 h-12 bg-gradient-to-br from-cyan-400 to-blue-600 rounded-xl flex items-center justify-center shadow-lg">
              <svg className="w-7 h-7 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <div>
              <h1 className="text-3xl font-bold bg-gradient-to-r from-cyan-300 to-blue-300 bg-clip-text text-transparent">
                SciDigest
              </h1>
              <p className="dark:text-gray-300 text-gray-700 text-sm font-medium">AI Research Paper Summarizer</p>
            </div>
          </div>
          
          <div className="flex items-center space-x-6 text-sm dark:text-gray-300 text-gray-700">
            <nav className="hidden md:flex items-center space-x-4">
              <a 
                href="/" 
                className="px-3 py-2 rounded-lg hover:dark:bg-white/10 hover:bg-black/10 transition-all duration-300"
              >
                Home
              </a>
              <a 
                href="/search" 
                className="px-3 py-2 rounded-lg hover:dark:bg-white/10 hover:bg-black/10 transition-all duration-300"
              >
                Search Papers
              </a>
              <a 
                href="/reading-lists" 
                className="px-3 py-2 rounded-lg hover:dark:bg-white/10 hover:bg-black/10 transition-all duration-300"
              >
                Reading Lists
              </a>
            </nav>
            <div className="hidden md:flex items-center space-x-2">
              <div className="w-2 h-2 bg-emerald-400 rounded-full animate-pulse"></div>
              <span>Online</span>
            </div>
            <div className="hidden md:block px-3 py-1 dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border rounded-full dark:text-gray-300 text-gray-700">
              ✨ Powered by AI
            </div>
            <ThemeToggle />
          </div>
        </div>
      </div>
    </header>
  );
}
