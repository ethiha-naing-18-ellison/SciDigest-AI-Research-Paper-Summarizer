export default function Spinner() {
  return (
    <div className="animate-spin rounded-full h-5 w-5 border-2 border-white/30 border-t-white" aria-label="Loading">
      <span className="sr-only">Loading...</span>
    </div>
  );
}
