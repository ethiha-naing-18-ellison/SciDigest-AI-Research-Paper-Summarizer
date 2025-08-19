export type PaperStatus = "Uploaded" | "Processing" | "Completed" | "Failed";

export interface SectionHeaderDto {
  name: string;
  orderIdx: number;
  tokens: number;
  pageStart: number;
  pageEnd: number;
}

export interface AnchorDto {
  bulletIndex: number;
  sectionName: string;
  pageStart: number;
  pageEnd: number;
}

export interface ContributionsDto {
  bullets: string[];
  details?: string[];            // NEW
  anchors: AnchorDto[];
}

export interface RelatedItem {
  title: string;
  authors: string;
  venue?: string;
  year?: number;
  url?: string;
  reason: string;
  alternative_urls?: string[];  // Additional URLs for the same paper
}

export interface RelatedPayload {
  provider: "OpenAlex" | "SemanticScholar";
  items: RelatedItem[];
}

export interface PaperDto {
  id: string;
  title: string | null;
  authors: string | null;
  year: number | null;
  venue: string | null;
  pages: number;
  status: PaperStatus;
  sections?: SectionHeaderDto[];
  summary?: { executiveSummary: string };
  contributions?: ContributionsDto;
  related?: RelatedPayload;
}
