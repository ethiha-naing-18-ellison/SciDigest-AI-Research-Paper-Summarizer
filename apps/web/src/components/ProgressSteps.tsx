export default function ProgressSteps({ status }: { status: "Uploaded"|"Processing"|"Completed"|"Failed" }) {
  const steps = ["Uploaded", "Processing", "Completed"];
  const currentIdx = status === "Failed" ? 1 : steps.indexOf(status);
  
  const getStepIcon = (step: string, index: number) => {
    const isActive = index <= currentIdx;
    const isCurrent = index === currentIdx && status !== "Failed";
    
    if (step === "Uploaded") {
      return (
        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
        </svg>
      );
    } else if (step === "Processing") {
      return (
        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
      );
    } else {
      return (
        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
        </svg>
      );
    }
  };

  return (
    <div className="flex items-center gap-4">
      {steps.map((s, i) => {
        const isActive = i <= currentIdx;
        const isCurrent = i === currentIdx && status !== "Failed";
        
        return (
          <div key={s} className="flex items-center gap-3">
            {/* Step indicator */}
            <div className="flex items-center gap-2">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center transition-all duration-300 ${
                isActive 
                  ? "bg-gradient-to-r from-cyan-500 to-blue-600 text-white shadow-lg" 
                  : "bg-white/10 text-white/50"
              } ${isCurrent ? "animate-pulse" : ""}`}>
                {getStepIcon(s, i)}
              </div>
              <div className={`text-sm font-medium transition-colors duration-300 ${
                isActive ? "text-white" : "text-white/50"
              }`}>
                {s}
              </div>
            </div>
            
            {/* Connector line */}
            {i < steps.length - 1 && (
              <div className={`w-12 h-0.5 transition-colors duration-300 ${
                i < currentIdx ? "bg-gradient-to-r from-cyan-500 to-blue-600" : "bg-white/20"
              }`} />
            )}
          </div>
        );
      })}
      
      {/* Failed indicator */}
      {status === "Failed" && (
        <div className="ml-4 inline-flex items-center gap-2 px-3 py-1 bg-red-500/20 border border-red-500/30 rounded-full">
          <svg className="w-4 h-4 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
          <span className="text-red-300 text-xs font-medium">Failed</span>
        </div>
      )}
    </div>
  );
}
