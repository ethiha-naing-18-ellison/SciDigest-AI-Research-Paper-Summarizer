export default function EmptyState({ title, desc }: { title: string; desc: string }) {
  return (
    <div className="glass rounded-xl border border-dashed border-white/20 p-8 text-center">
      <div className="w-12 h-12 mx-auto mb-4 bg-gradient-to-br from-red-400 to-pink-500 rounded-full flex items-center justify-center">
        <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.664-.833-2.464 0L4.268 16.5c-.77.833.192 2.5 1.732 2.5z" />
        </svg>
      </div>
      <div className="font-medium text-gray-100 text-lg mb-2">{title}</div>
      <div className="text-gray-300">{desc}</div>
    </div>
  );
}
