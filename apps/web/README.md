# SciDigest Web Frontend

A production-ready Next.js 14 frontend for the AI Research Paper Summarizer.

## Features

- **Upload Interface**: Drag & drop PDF upload with progress feedback
- **Real-time Processing**: Auto-polling status updates while papers are processed
- **Rich Results Display**: Executive summary, key contributions with page anchors, and related work suggestions
- **Export Functionality**: Download summaries as Markdown or PDF
- **Responsive Design**: Clean, modern UI built with Tailwind CSS
- **Type Safety**: Full TypeScript integration with strict mode
- **Error Handling**: Comprehensive error states and user feedback

## Tech Stack

- **Next.js 14** (App Router)
- **TypeScript** (strict mode)
- **Tailwind CSS** for styling
- **SWR** for data fetching and polling
- **React Hook Form** for form handling

## Prerequisites

- Node.js 18+ 
- npm, yarn, or pnpm
- Backend API running (see `/apps/api`)

## Setup

1. **Install dependencies**:
   ```bash
   cd apps/web
   npm install
   # or
   yarn install
   # or
   pnpm install
   ```

2. **Configure environment**:
   ```bash
   cp .env.local.example .env.local
   ```
   
   Edit `.env.local` and set your backend URL:
   ```
   NEXT_PUBLIC_API_BASE=http://localhost:5108
   ```

3. **Start development server**:
   ```bash
   npm run dev
   # or
   yarn dev
   # or
   pnpm dev
   ```

4. **Open your browser** to [http://localhost:3000](http://localhost:3000)

## Project Structure

```
src/
├── app/                    # Next.js App Router pages
│   ├── layout.tsx         # Root layout
│   ├── page.tsx          # Home page (upload)
│   └── paper/[id]/
│       └── page.tsx      # Results page
├── components/            # Reusable UI components
│   ├── UploadDropzone.tsx
│   ├── ProgressSteps.tsx
│   ├── SummaryCard.tsx
│   ├── ContributionsList.tsx
│   ├── RelatedGrid.tsx
│   ├── ExportButtons.tsx
│   ├── EmptyState.tsx
│   ├── Toast.tsx
│   └── Spinner.tsx
├── lib/                   # Utility functions
│   ├── api.ts            # API client functions
│   ├── urls.ts           # API endpoint builders
│   └── utils.ts          # Helper utilities
├── types/
│   └── dto.ts            # TypeScript type definitions
└── hooks/
    └── usePollPaper.ts   # SWR polling hook
```

## Pages

- **`/`** - Upload page with drag & drop interface
- **`/paper/[id]`** - Results page with automatic status polling

## API Integration

The frontend integrates with the backend API through:

- **Upload**: `POST /api/papers` (multipart/form-data)
- **Status**: `GET /api/papers/{id}` (polling every 2s during processing)
- **Reprocess**: `POST /api/papers/{id}/process`
- **Export**: `POST /api/papers/{id}/export?format={md|pdf}`

## Build & Deploy

1. **Build for production**:
   ```bash
   npm run build
   ```

2. **Start production server**:
   ```bash
   npm start
   ```

3. **Lint code**:
   ```bash
   npm run lint
   ```

## Key Features Explained

### Upload Flow
1. User drags/drops or selects a PDF file
2. File is validated (PDF only, size limits)
3. Uploaded via multipart form to backend
4. User is automatically redirected to results page

### Processing Flow
1. Results page polls paper status every 2 seconds
2. Progress steps show: Uploaded → Processing → Completed
3. Failed states show retry button
4. Completed state displays all results

### Results Display
- **Executive Summary**: Copyable text summary
- **Key Contributions**: Bulleted list with page anchors on hover
- **Related Work**: Grid of suggested papers with links and rationale
- **Export Options**: Download as Markdown or PDF

### Error Handling
- Toast notifications for user feedback
- Empty states for missing data
- Loading spinners during async operations
- Comprehensive error boundaries

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_BASE` | Backend API base URL | `http://localhost:5108` |

## CORS Requirements

Ensure your backend API allows requests from:
- `http://localhost:3000` (development)
- Your production domain (production)

## Contributing

1. Follow the existing code style and patterns
2. Ensure TypeScript strict mode compliance
3. Add appropriate error handling
4. Test upload and polling flows
5. Verify responsive design on mobile devices

## License

Private - SciDigest Project
