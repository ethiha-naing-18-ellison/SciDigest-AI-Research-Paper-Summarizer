"use client";

import { useState, useEffect } from "react";
import { searchPapers, getFilterOptions, type SearchParams, type SearchResponse, type FilterOptions } from "@/lib/api";
import type { PaperDto } from "@/types/dto";
import { useRouter } from "next/navigation";

export default function PaperSearch() {
  const router = useRouter();
  const [searchResults, setSearchResults] = useState<SearchResponse | null>(null);
  const [filterOptions, setFilterOptions] = useState<FilterOptions | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Search form state
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedVenue, setSelectedVenue] = useState("");
  const [selectedYear, setSelectedYear] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("");
  const [sortBy, setSortBy] = useState("createdAt");
  const [sortDescending, setSortDescending] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  // Load filter options on mount
  useEffect(() => {
    const loadFilterOptions = async () => {
      try {
        const options = await getFilterOptions();
        setFilterOptions(options);
      } catch (err) {
        console.error("Failed to load filter options:", err);
      }
    };
    loadFilterOptions();
  }, []);

  // Perform search
  const handleSearch = async (page = 1) => {
    setLoading(true);
    setError(null);
    
    try {
      const params: SearchParams = {
        searchTerm: searchTerm || undefined,
        venue: selectedVenue || undefined,
        year: selectedYear ? parseInt(selectedYear) : undefined,
        status: selectedStatus || undefined,
        sortBy,
        sortDescending,
        skip: (page - 1) * pageSize,
        take: pageSize,
      };

      const results = await searchPapers(params);
      setSearchResults(results);
      setCurrentPage(page);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Search failed");
    } finally {
      setLoading(false);
    }
  };

  // Handle form submission
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleSearch(1);
  };

  // Reset filters
  const resetFilters = () => {
    setSearchTerm("");
    setSelectedVenue("");
    setSelectedYear("");
    setSelectedStatus("");
    setSortBy("createdAt");
    setSortDescending(true);
    setCurrentPage(1);
    setSearchResults(null);
    setError(null);
  };

  // Navigate to paper details
  const viewPaper = (paperId: string) => {
    router.push(`/papers/${paperId}`);
  };

  // Pagination
  const totalPages = searchResults?.totalPages || 0;
  const startPage = Math.max(1, currentPage - 2);
  const endPage = Math.min(totalPages, startPage + 4);

  return (
    <div className="space-y-6">
      {/* Search Header */}
      <div className="glass rounded-3xl p-8 shadow-2xl">
        <div className="flex items-center space-x-3 mb-6">
          <div className="w-8 h-8 bg-gradient-to-br from-emerald-400 to-cyan-500 rounded-lg flex items-center justify-center">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <h2 className="text-2xl font-bold dark:text-gray-100 text-gray-900">Search Research Papers</h2>
        </div>

        {/* Search Form */}
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Search Input */}
          <div>
            <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
              Search Term
            </label>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search by title, authors, or content..."
              className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-cyan-500 focus:border-transparent transition-all"
            />
          </div>

          {/* Filters Row */}
          <div className="grid md:grid-cols-4 gap-4">
            {/* Venue Filter */}
            <div>
              <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                Venue
              </label>
              <select
                value={selectedVenue}
                onChange={(e) => setSelectedVenue(e.target.value)}
                className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-cyan-500 focus:border-transparent transition-all"
              >
                <option value="">All Venues</option>
                {filterOptions?.venues.map((venue) => (
                  <option key={venue} value={venue}>{venue}</option>
                ))}
              </select>
            </div>

            {/* Year Filter */}
            <div>
              <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                Year
              </label>
              <select
                value={selectedYear}
                onChange={(e) => setSelectedYear(e.target.value)}
                className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-cyan-500 focus:border-transparent transition-all"
              >
                <option value="">All Years</option>
                {filterOptions?.years.map((year) => (
                  <option key={year} value={year}>{year}</option>
                ))}
              </select>
            </div>

            {/* Status Filter */}
            <div>
              <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                Status
              </label>
              <select
                value={selectedStatus}
                onChange={(e) => setSelectedStatus(e.target.value)}
                className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-cyan-500 focus:border-transparent transition-all"
              >
                <option value="">All Statuses</option>
                {filterOptions?.statuses.map((status) => (
                  <option key={status} value={status}>{status}</option>
                ))}
              </select>
            </div>

            {/* Sort Options */}
            <div>
              <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                Sort By
              </label>
              <select
                value={`${sortBy}-${sortDescending}`}
                onChange={(e) => {
                  const [field, desc] = e.target.value.split('-');
                  setSortBy(field);
                  setSortDescending(desc === 'true');
                }}
                className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-cyan-500 focus:border-transparent transition-all"
              >
                <option value="createdAt-true">Newest First</option>
                <option value="createdAt-false">Oldest First</option>
                <option value="title-false">Title A-Z</option>
                <option value="title-true">Title Z-A</option>
                <option value="year-true">Year (Recent)</option>
                <option value="year-false">Year (Old)</option>
                <option value="authors-false">Authors A-Z</option>
                <option value="venue-false">Venue A-Z</option>
              </select>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-wrap gap-4">
            <button
              type="submit"
              disabled={loading}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-xl font-medium hover:from-cyan-600 hover:to-blue-700 transition-all duration-300 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg"
            >
              {loading ? (
                <>
                  <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                  Searching...
                </>
              ) : (
                <>
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                  Search Papers
                </>
              )}
            </button>

            <button
              type="button"
              onClick={resetFilters}
              className="flex items-center gap-2 px-6 py-3 dark:bg-white/10 bg-black/10 dark:text-gray-300 text-gray-700 rounded-xl font-medium hover:dark:bg-white/20 hover:bg-black/20 transition-all duration-300"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Reset Filters
            </button>
          </div>
        </form>
      </div>

      {/* Error Message */}
      {error && (
        <div className="glass rounded-xl p-4 border-l-4 border-red-500">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <svg className="w-5 h-5 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm dark:text-red-300 text-red-700">{error}</p>
            </div>
          </div>
        </div>
      )}

      {/* Search Results */}
      {searchResults && (
        <div className="space-y-6">
          {/* Results Header */}
          <div className="glass rounded-xl p-4">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-lg font-semibold dark:text-gray-100 text-gray-900">
                  Search Results
                </h3>
                <p className="text-sm dark:text-gray-400 text-gray-600">
                  Found {searchResults.totalCount} papers • Page {currentPage} of {totalPages}
                </p>
              </div>
            </div>
          </div>

          {/* Results List */}
          {searchResults.papers.length > 0 ? (
            <div className="space-y-4">
              {searchResults.papers.map((paper) => (
                <div
                  key={paper.id}
                  className="glass rounded-xl p-6 hover:dark:bg-white/10 hover:bg-black/10 transition-all duration-300 cursor-pointer transform hover:scale-[1.02]"
                  onClick={() => viewPaper(paper.id)}
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <h4 className="text-lg font-semibold dark:text-gray-100 text-gray-900 mb-2">
                        {paper.title || "Untitled Paper"}
                      </h4>
                      
                      {paper.authors && (
                        <p className="text-sm dark:text-gray-400 text-gray-600 mb-2">
                          <span className="font-medium">Authors:</span> {paper.authors}
                        </p>
                      )}
                      
                      <div className="flex flex-wrap gap-4 text-sm dark:text-gray-400 text-gray-600 mb-3">
                        {paper.year && (
                          <span><span className="font-medium">Year:</span> {paper.year}</span>
                        )}
                        {paper.venue && (
                          <span><span className="font-medium">Venue:</span> {paper.venue}</span>
                        )}
                        <span><span className="font-medium">Pages:</span> {paper.pages}</span>
                      </div>

                      {paper.summary?.executiveSummary && (
                        <p className="text-sm dark:text-gray-300 text-gray-700 line-clamp-2">
                          {paper.summary.executiveSummary}
                        </p>
                      )}
                    </div>

                    <div className="ml-4 flex flex-col items-end gap-2">
                      <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                        paper.status === 'Completed' 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
                          : paper.status === 'Processing'
                          ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-300'
                          : paper.status === 'Failed'
                          ? 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                          : 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-300'
                      }`}>
                        {paper.status}
                      </span>
                      
                      <button className="flex items-center gap-1 text-xs dark:text-cyan-400 text-cyan-600 hover:dark:text-cyan-300 hover:text-cyan-700 transition-colors">
                        <span>View Details</span>
                        <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="glass rounded-xl p-8 text-center">
              <div className="w-16 h-16 bg-gradient-to-br from-gray-400 to-gray-600 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold dark:text-gray-100 text-gray-900 mb-2">
                No papers found
              </h3>
              <p className="dark:text-gray-400 text-gray-600">
                Try adjusting your search criteria or filters
              </p>
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="glass rounded-xl p-4">
              <div className="flex items-center justify-center space-x-2">
                {/* Previous Button */}
                <button
                  onClick={() => handleSearch(currentPage - 1)}
                  disabled={currentPage === 1}
                  className="flex items-center gap-1 px-3 py-2 rounded-lg dark:bg-white/10 bg-black/10 dark:text-gray-300 text-gray-700 hover:dark:bg-white/20 hover:bg-black/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                  </svg>
                  Previous
                </button>

                {/* Page Numbers */}
                {Array.from({ length: endPage - startPage + 1 }, (_, i) => startPage + i).map((page) => (
                  <button
                    key={page}
                    onClick={() => handleSearch(page)}
                    className={`px-3 py-2 rounded-lg transition-all ${
                      page === currentPage
                        ? 'bg-gradient-to-r from-cyan-500 to-blue-600 text-white'
                        : 'dark:bg-white/10 bg-black/10 dark:text-gray-300 text-gray-700 hover:dark:bg-white/20 hover:bg-black/20'
                    }`}
                  >
                    {page}
                  </button>
                ))}

                {/* Next Button */}
                <button
                  onClick={() => handleSearch(currentPage + 1)}
                  disabled={currentPage === totalPages}
                  className="flex items-center gap-1 px-3 py-2 rounded-lg dark:bg-white/10 bg-black/10 dark:text-gray-300 text-gray-700 hover:dark:bg-white/20 hover:bg-black/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Next
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
