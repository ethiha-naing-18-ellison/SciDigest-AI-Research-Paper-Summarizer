"use client";

import { useParams } from "next/navigation";
import { usePollPaper } from "@/hooks/usePollPaper";
import ProgressSteps from "@/components/ProgressSteps";
import SummaryCard from "@/components/SummaryCard";
import ContributionsList from "@/components/ContributionsList";
import RelatedGrid from "@/components/RelatedGrid";
import ExportButtons from "@/components/ExportButtons";
import { reprocess } from "@/lib/api";
import Spinner from "@/components/Spinner";
import EmptyState from "@/components/EmptyState";
import { useState } from "react";
import { Toast } from "@/components/Toast";

export default function PaperPage() {
  const params = useParams<{ id: string }>();
  const id = params.id;
  const { paper, error, isLoading, mutate } = usePollPaper(id);
  const [toast, setToast] = useState<string | null>(null);

  if (error) return <EmptyState title="Error" desc="Unable to load paper. Please go back and try again." />;
  if (!paper || isLoading) return <div className="flex items-center gap-2"><Spinner /><span className="text-sm text-gray-300">Loading…</span></div>;

  const failed = paper.status === "Failed";
  const done = paper.status === "Completed";

  async function onRetry() {
    try {
      await reprocess(id);
      setToast("Reprocessing started.");
      mutate();
    } catch (e: any) {
      setToast(e.message || "Failed to reprocess.");
    }
  }

  return (
    <main className="space-y-6">
      {toast && <Toast message={toast} onDone={() => setToast(null)} />}
      <div className="glass rounded-2xl p-6 shadow-2xl">
        <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <div className="text-xs uppercase text-gray-400 tracking-wider">Paper</div>
            <div className="text-lg font-semibold text-gray-100">{paper.title || "Untitled"}</div>
            <div className="text-sm text-gray-300">
              {paper.authors || "Unknown"} {paper.venue ? `• ${paper.venue}` : ""} {paper.year ? `• ${paper.year}` : ""}
            </div>
          </div>
          <ProgressSteps status={paper.status} />
        </div>
        {failed && (
          <div className="mt-4">
            <button onClick={onRetry} className="rounded-xl bg-gradient-to-r from-red-500 to-red-600 px-4 py-2 text-white hover:from-red-600 hover:to-red-700 transition-all duration-300 transform hover:scale-105">
              Retry processing
            </button>
          </div>
        )}
      </div>

      {!done && !failed && (
        <div className="glass rounded-2xl p-6 shadow-2xl">
          <div className="flex items-center gap-2"><Spinner /><div className="text-sm text-gray-300">Working on your paper…</div></div>
          <div className="mt-2 text-xs text-gray-400">This page will refresh automatically.</div>
        </div>
      )}

      {done && (
        <>
          <SummaryCard text={paper.summary?.executiveSummary} />
          <ContributionsList bullets={paper.contributions?.bullets} details={paper.contributions?.details} anchors={paper.contributions?.anchors} />
          <RelatedGrid data={paper.related} />
          <div className="glass rounded-2xl p-5 shadow-2xl">
            <ExportButtons id={paper.id} />
          </div>
        </>
      )}
    </main>
  );
}
