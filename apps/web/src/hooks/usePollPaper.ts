"use client";

import useSWR from "swr";
import { getPaper } from "@/lib/api";
import type { PaperDto } from "@/types/dto";

export function usePollPaper(id: string) {
  const { data, error, isLoading, mutate } = useSWR<PaperDto>(
    id ? ["paper", id] : null,
    () => getPaper(id),
    { refreshInterval: (data) => (data?.status === "Processing" ? 2000 : 0) as unknown as number }
  );
  return { paper: data, error, isLoading, mutate };
}
