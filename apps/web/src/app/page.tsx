import UploadDropzone from "@/components/UploadDropzone";
import DebugEnv from "@/components/DebugEnv";

export default function HomePage() {
  return (
    <div className="space-y-8">
      {/* Hero Section */}
      <div className="text-center space-y-4 py-8">
        <div className="inline-flex items-center space-x-2 px-4 py-2 bg-white/5 border border-white/10 rounded-full text-cyan-200 text-sm font-medium">
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
          </svg>
          <span>AI-Powered Research Assistant</span>
        </div>
        <h1 className="text-4xl md:text-6xl font-bold bg-gradient-to-r from-cyan-300 via-blue-300 to-purple-300 bg-clip-text text-transparent">
          Transform Research Papers
        </h1>
        <p className="text-xl text-gray-300 max-w-2xl mx-auto">
          Upload any PDF research paper and get instant AI-generated summaries, key contributions, and related work suggestions
        </p>
      </div>

      {/* Upload Section */}
      <div className="glass rounded-3xl p-8 shadow-2xl">
        <div className="flex items-center space-x-3 mb-6">
          <div className="w-8 h-8 bg-gradient-to-br from-emerald-400 to-cyan-500 rounded-lg flex items-center justify-center">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
          </div>
          <h2 className="text-2xl font-bold text-gray-100">Start by uploading a PDF</h2>
        </div>
        <p className="text-gray-400 mb-6">
          Max 50MB. We&apos;ll parse sections, summarize, and suggest related work using advanced AI.
        </p>
        <UploadDropzone />
      </div>

      {/* How it Works Section */}
      <div className="glass rounded-3xl p-8 shadow-2xl">
        <div className="flex items-center space-x-3 mb-6">
          <div className="w-8 h-8 bg-gradient-to-br from-purple-400 to-pink-500 rounded-lg flex items-center justify-center">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
            </svg>
          </div>
          <h3 className="text-2xl font-bold text-gray-100">How it works</h3>
        </div>
        
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[
            {
              icon: (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
              ),
              title: "Upload Paper",
              description: "Upload your research paper (PDF format, up to 50MB)",
              color: "from-blue-400 to-blue-600"
            },
            {
              icon: (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              ),
              title: "AI Processing",
              description: "Our pipeline extracts sections and analyzes content",
              color: "from-purple-400 to-purple-600"
            },
            {
              icon: (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                </svg>
              ),
              title: "Generate Summary",
              description: "Get executive summary and key contributions",
              color: "from-green-400 to-green-600"
            },
            {
              icon: (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
                </svg>
              ),
              title: "Related Work",
              description: "Find related papers from OpenAlex & Semantic Scholar",
              color: "from-orange-400 to-orange-600"
            },
            {
              icon: (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              ),
              title: "Export Results",
              description: "Download as Markdown or PDF format",
              color: "from-pink-400 to-pink-600"
            }
          ].map((step, index) => (
            <div key={index} className="bg-white/5 border border-white/10 rounded-xl p-6 hover:bg-white/10 transition-all duration-300 transform hover:scale-105">
              <div className={`w-12 h-12 bg-gradient-to-br ${step.color} rounded-xl flex items-center justify-center mb-4 shadow-lg`}>
                {step.icon}
              </div>
              <h4 className="text-gray-100 font-semibold mb-2">{step.title}</h4>
              <p className="text-gray-400 text-sm">{step.description}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Features Section */}
      <div className="grid md:grid-cols-3 gap-6">
        {[
          {
            icon: "⚡",
            title: "Lightning Fast",
            description: "Get results in minutes, not hours"
          },
          {
            icon: "🎯",
            title: "Highly Accurate",
            description: "AI-powered analysis with precision"
          },
          {
            icon: "🔒",
            title: "Secure & Private",
            description: "Your data is safe and protected"
          }
        ].map((feature, index) => (
          <div key={index} className="glass rounded-xl p-6 text-center hover:scale-105 transition-transform duration-300">
            <div className="text-3xl mb-3">{feature.icon}</div>
            <h3 className="text-gray-100 font-semibold mb-2">{feature.title}</h3>
            <p className="text-gray-400 text-sm">{feature.description}</p>
          </div>
        ))}
      </div>

      <DebugEnv />
    </div>
  );
}
