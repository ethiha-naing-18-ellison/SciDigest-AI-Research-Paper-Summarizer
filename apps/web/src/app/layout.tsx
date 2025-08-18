import "./globals.css";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "SciDigest – AI Research Paper Summarizer",
  description: "Upload a research PDF to get an executive summary, contributions, and related work.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="min-h-screen gradient-bg text-gray-100 antialiased">
        {/* Background decorative elements */}
        <div className="fixed inset-0 overflow-hidden pointer-events-none">
          <div className="absolute -top-40 -right-40 w-80 h-80 bg-blue-500/20 rounded-full blur-3xl float-animation"></div>
          <div className="absolute -bottom-40 -left-40 w-96 h-96 bg-purple-500/10 rounded-full blur-3xl" style={{animationDelay: '3s'}}></div>
          <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-64 h-64 bg-cyan-500/15 rounded-full blur-2xl float-animation" style={{animationDelay: '1.5s'}}></div>
        </div>

        <div className="relative z-10 mx-auto max-w-6xl p-6">
          {/* Enhanced Header */}
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
                    <p className="text-gray-300 text-sm font-medium">AI Research Paper Summarizer</p>
                  </div>
                </div>
                
                <div className="hidden md:flex items-center space-x-6 text-sm text-gray-300">
                  <div className="flex items-center space-x-2">
                    <div className="w-2 h-2 bg-emerald-400 rounded-full animate-pulse"></div>
                    <span>Online</span>
                  </div>
                  <div className="px-3 py-1 bg-white/5 border border-white/10 rounded-full">
                    ✨ Powered by AI
                  </div>
                </div>
              </div>
            </div>
          </header>

          {/* Content with glass container */}
          <main className="space-y-6">
            {children}
          </main>

          {/* Enhanced Footer */}
          <footer className="mt-16 text-center">
            <div className="glass rounded-xl p-4 inline-block">
              <p className="text-gray-400 text-sm">
                © {new Date().getFullYear()} SciDigest • Made with ❤️ for researchers worldwide
              </p>
            </div>
          </footer>
        </div>
      </body>
    </html>
  );
}
