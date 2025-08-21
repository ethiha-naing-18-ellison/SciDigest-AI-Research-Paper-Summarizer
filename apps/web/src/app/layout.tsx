import "./globals.css";
import type { Metadata } from "next";
import { ThemeProvider } from "@/context/ThemeContext";
import Header from "@/components/Header";

export const metadata: Metadata = {
  title: "SciDigest – AI Research Paper Summarizer",
  description: "Upload a research PDF to get an executive summary, contributions, and related work.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <script
          dangerouslySetInnerHTML={{
            __html: `
              (function() {
                try {
                  var theme = localStorage.getItem('scidigest-theme') || 'dark';
                  document.documentElement.classList.add(theme);
                } catch (e) {
                  document.documentElement.classList.add('dark');
                }
              })();
            `,
          }}
        />
      </head>
      <body className="min-h-screen gradient-bg dark:text-gray-100 text-gray-900 antialiased">
        <ThemeProvider>
        {/* Background decorative elements */}
        <div className="fixed inset-0 overflow-hidden pointer-events-none">
          <div className="absolute -top-40 -right-40 w-80 h-80 dark:bg-blue-500/20 bg-blue-400/30 rounded-full blur-3xl float-animation"></div>
          <div className="absolute -bottom-40 -left-40 w-96 h-96 dark:bg-purple-500/10 bg-purple-400/20 rounded-full blur-3xl" style={{animationDelay: '3s'}}></div>
          <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-64 h-64 dark:bg-cyan-500/15 bg-cyan-400/25 rounded-full blur-2xl float-animation" style={{animationDelay: '1.5s'}}></div>
        </div>

        <div className="relative z-10 mx-auto max-w-6xl p-6">
          <Header />

          {/* Content with glass container */}
          <main className="space-y-6">
            {children}
          </main>

          {/* Enhanced Footer */}
          <footer className="mt-16 text-center">
            <div className="glass rounded-xl p-4 inline-block">
              <p className="dark:text-gray-400 text-gray-600 text-sm">
                © {new Date().getFullYear()} SciDigest • Made with ❤️ for researchers worldwide
              </p>
            </div>
          </footer>
        </div>
        </ThemeProvider>
      </body>
    </html>
  );
}
