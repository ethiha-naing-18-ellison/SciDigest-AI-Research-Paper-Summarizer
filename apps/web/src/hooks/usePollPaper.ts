"use client";

import useSWR from "swr";
import { getPaper } from "@/lib/api";
import type { PaperDto } from "@/types/dto";
import { useEffect, useRef } from "react";

export function usePollPaper(id: string) {
  const { data, error, isLoading, mutate } = useSWR<PaperDto>(
    id ? ["paper", id] : null,
    () => getPaper(id),
    {
      refreshInterval: (data) => (data?.status === "Processing" ? 2000 : 0) as unknown as number,
      revalidateOnFocus: true,
      revalidateOnReconnect: true,
      revalidateIfStale: true
    }
  );

  // one forced refresh when status flips to Completed
  const lastStatus = useRef<string | null>(null);
  useEffect(() => {
    if (!data) return;
    if (lastStatus.current !== "Completed" && data.status === "Completed") {
      lastStatus.current = "Completed";
      // force one more fetch to ensure latest meta is seen
      mutate();
    } else {
      lastStatus.current = data.status;
    }
  }, [data, mutate]);

  return { paper: data, error, isLoading, mutate };
}
