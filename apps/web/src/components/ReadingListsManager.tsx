"use client";

import { useState, useEffect } from "react";
import { 
  getReadingLists, 
  createReadingList, 
  deleteReadingList, 
  updateReadingList,
  addPaperToList,
  removePaperFromList,
  updateItemNotes,
  type ReadingList, 
  type CreateReadingListRequest,
  type UpdateReadingListRequest,
  type AddPaperToListRequest
} from "@/lib/api";
import { useRouter } from "next/navigation";

export default function ReadingListsManager() {
  const router = useRouter();
  const [readingLists, setReadingLists] = useState<ReadingList[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Create/Edit modal state
  const [showModal, setShowModal] = useState(false);
  const [editingList, setEditingList] = useState<ReadingList | null>(null);
  const [formData, setFormData] = useState({ name: "", description: "" });
  
  // Add paper modal state
  const [showAddPaperModal, setShowAddPaperModal] = useState(false);
  const [selectedListId, setSelectedListId] = useState<string>("");
  const [paperForm, setPaperForm] = useState({ paperId: "", notes: "" });

  // Load reading lists on mount
  useEffect(() => {
    loadReadingLists();
  }, []);

  const loadReadingLists = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const lists = await getReadingLists();
      setReadingLists(lists);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load reading lists");
    } finally {
      setLoading(false);
    }
  };

  const handleCreateList = () => {
    setEditingList(null);
    setFormData({ name: "", description: "" });
    setShowModal(true);
  };

  const handleEditList = (list: ReadingList) => {
    setEditingList(list);
    setFormData({ name: list.name, description: list.description || "" });
    setShowModal(true);
  };

  const handleSaveList = async () => {
    if (!formData.name.trim()) {
      setError("Please enter a name for the reading list");
      return;
    }

    try {
      if (editingList) {
        const request: UpdateReadingListRequest = {
          name: formData.name.trim(),
          description: formData.description.trim() || undefined,
        };
        await updateReadingList(editingList.id, request);
      } else {
        const request: CreateReadingListRequest = {
          name: formData.name.trim(),
          description: formData.description.trim() || undefined,
        };
        await createReadingList(request);
      }
      
      setShowModal(false);
      setFormData({ name: "", description: "" });
      setEditingList(null);
      await loadReadingLists();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save reading list");
    }
  };

  const handleDeleteList = async (listId: string) => {
    if (!confirm("Are you sure you want to delete this reading list? This action cannot be undone.")) {
      return;
    }

    try {
      await deleteReadingList(listId);
      await loadReadingLists();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete reading list");
    }
  };

  const handleAddPaper = (listId: string) => {
    setSelectedListId(listId);
    setPaperForm({ paperId: "", notes: "" });
    setShowAddPaperModal(true);
  };

  const handleSavePaper = async () => {
    if (!paperForm.paperId.trim()) {
      setError("Please enter a paper ID");
      return;
    }

    try {
      const request: AddPaperToListRequest = {
        paperId: paperForm.paperId.trim(),
        notes: paperForm.notes.trim() || undefined,
      };
      await addPaperToList(selectedListId, request);
      
      setShowAddPaperModal(false);
      setPaperForm({ paperId: "", notes: "" });
      setSelectedListId("");
      await loadReadingLists();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to add paper to list");
    }
  };

  const handleRemovePaper = async (listId: string, paperId: string) => {
    if (!confirm("Remove this paper from the reading list?")) {
      return;
    }

    try {
      await removePaperFromList(listId, paperId);
      await loadReadingLists();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove paper");
    }
  };

  const viewPaper = (paperId: string) => {
    router.push(`/papers/${paperId}`);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="glass rounded-3xl p-8 shadow-2xl">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-gradient-to-br from-purple-400 to-pink-500 rounded-lg flex items-center justify-center">
              <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
            </div>
            <div>
              <h2 className="text-2xl font-bold dark:text-gray-100 text-gray-900">Reading Lists</h2>
              <p className="dark:text-gray-400 text-gray-700">Organize and manage your research papers</p>
            </div>
          </div>
          
          <button
            onClick={handleCreateList}
            className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-purple-500 to-pink-600 text-white rounded-xl font-medium hover:from-purple-600 hover:to-pink-700 transition-all duration-300 transform hover:scale-105 shadow-lg"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            New Reading List
          </button>
        </div>
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
              <button
                onClick={() => setError(null)}
                className="text-xs dark:text-red-400 text-red-600 hover:underline mt-1"
              >
                Dismiss
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div className="glass rounded-xl p-8 text-center">
          <div className="w-8 h-8 border-2 border-purple-300 border-t-purple-600 rounded-full animate-spin mx-auto mb-4"></div>
          <p className="dark:text-gray-300 text-gray-700">Loading reading lists...</p>
        </div>
      )}

      {/* Reading Lists Grid */}
      {!loading && (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {readingLists.map((list) => (
            <div key={list.id} className="glass rounded-xl p-6 hover:dark:bg-white/10 hover:bg-black/10 transition-all duration-300">
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <h3 className="text-lg font-semibold dark:text-gray-100 text-gray-900 mb-2">
                    {list.name}
                  </h3>
                  {list.description && (
                    <p className="text-sm dark:text-gray-400 text-gray-600 mb-3 line-clamp-2">
                      {list.description}
                    </p>
                  )}
                  <div className="flex items-center gap-4 text-xs dark:text-gray-500 text-gray-500">
                    <span>{list.items.length} papers</span>
                    <span>Updated {new Date(list.updatedAt).toLocaleDateString()}</span>
                  </div>
                </div>
                
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => handleEditList(list)}
                    className="p-2 rounded-lg dark:hover:bg-white/10 hover:bg-black/10 transition-colors"
                    title="Edit list"
                  >
                    <svg className="w-4 h-4 dark:text-gray-400 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                    </svg>
                  </button>
                  <button
                    onClick={() => handleDeleteList(list.id)}
                    className="p-2 rounded-lg dark:hover:bg-white/10 hover:bg-black/10 transition-colors"
                    title="Delete list"
                  >
                    <svg className="w-4 h-4 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </div>

              {/* Papers in List */}
              <div className="space-y-3">
                {list.items.slice(0, 3).map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between p-3 dark:bg-white/5 bg-black/5 rounded-lg cursor-pointer hover:dark:bg-white/10 hover:bg-black/10 transition-colors"
                    onClick={() => viewPaper(item.paperId)}
                  >
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium dark:text-gray-200 text-gray-800 truncate">
                        {item.paper?.title || `Paper ${item.paperId.slice(0, 8)}...`}
                      </p>
                      {item.notes && (
                        <p className="text-xs dark:text-gray-500 text-gray-500 mt-1 line-clamp-1">
                          {item.notes}
                        </p>
                      )}
                    </div>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        handleRemovePaper(list.id, item.paperId);
                      }}
                      className="ml-2 p-1 rounded dark:hover:bg-white/10 hover:bg-black/10 transition-colors"
                      title="Remove paper"
                    >
                      <svg className="w-3 h-3 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>
                ))}

                {list.items.length > 3 && (
                  <p className="text-xs dark:text-gray-500 text-gray-500 text-center">
                    +{list.items.length - 3} more papers
                  </p>
                )}

                {list.items.length === 0 && (
                  <p className="text-sm dark:text-gray-500 text-gray-500 text-center py-4">
                    No papers in this list yet
                  </p>
                )}
              </div>

              {/* Add Paper Button */}
              <button
                onClick={() => handleAddPaper(list.id)}
                className="w-full mt-4 py-2 px-4 border-2 border-dashed dark:border-gray-600 border-gray-300 rounded-lg dark:text-gray-400 text-gray-600 hover:dark:border-gray-500 hover:border-gray-400 hover:dark:text-gray-300 hover:text-gray-700 transition-colors text-sm"
              >
                + Add Paper
              </button>
            </div>
          ))}

          {readingLists.length === 0 && !loading && (
            <div className="col-span-full glass rounded-xl p-8 text-center">
              <div className="w-16 h-16 bg-gradient-to-br from-purple-400 to-pink-500 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold dark:text-gray-100 text-gray-900 mb-2">
                No reading lists yet
              </h3>
              <p className="dark:text-gray-400 text-gray-600 mb-4">
                Create your first reading list to start organizing papers
              </p>
              <button
                onClick={handleCreateList}
                className="px-6 py-3 bg-gradient-to-r from-purple-500 to-pink-600 text-white rounded-xl font-medium hover:from-purple-600 hover:to-pink-700 transition-all duration-300 transform hover:scale-105 shadow-lg"
              >
                Create Reading List
              </button>
            </div>
          )}
        </div>
      )}

      {/* Create/Edit Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="glass rounded-2xl p-6 w-full max-w-md">
            <h3 className="text-lg font-semibold dark:text-gray-100 text-gray-900 mb-4">
              {editingList ? "Edit Reading List" : "Create Reading List"}
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                  Name
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="Enter list name..."
                  className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all"
                  autoFocus
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                  Description (optional)
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Enter description..."
                  rows={3}
                  className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all resize-none"
                />
              </div>
            </div>

            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowModal(false)}
                className="flex-1 px-4 py-3 dark:bg-white/10 bg-black/10 dark:text-gray-300 text-gray-700 rounded-xl font-medium hover:dark:bg-white/20 hover:bg-black/20 transition-all"
              >
                Cancel
              </button>
              <button
                onClick={handleSaveList}
                className="flex-1 px-4 py-3 bg-gradient-to-r from-purple-500 to-pink-600 text-white rounded-xl font-medium hover:from-purple-600 hover:to-pink-700 transition-all duration-300"
              >
                {editingList ? "Update" : "Create"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Add Paper Modal */}
      {showAddPaperModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="glass rounded-2xl p-6 w-full max-w-md">
            <h3 className="text-lg font-semibold dark:text-gray-100 text-gray-900 mb-4">
              Add Paper to List
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                  Paper ID
                </label>
                <input
                  type="text"
                  value={paperForm.paperId}
                  onChange={(e) => setPaperForm({ ...paperForm, paperId: e.target.value })}
                  placeholder="Enter paper ID..."
                  className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all"
                  autoFocus
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium dark:text-gray-300 text-gray-700 mb-2">
                  Notes (optional)
                </label>
                <textarea
                  value={paperForm.notes}
                  onChange={(e) => setPaperForm({ ...paperForm, notes: e.target.value })}
                  placeholder="Add your notes..."
                  rows={3}
                  className="w-full px-4 py-3 rounded-xl dark:bg-white/5 bg-black/5 dark:border-white/10 border-gray-300 border dark:text-gray-100 text-gray-900 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all resize-none"
                />
              </div>
            </div>

            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowAddPaperModal(false)}
                className="flex-1 px-4 py-3 dark:bg-white/10 bg-black/10 dark:text-gray-300 text-gray-700 rounded-xl font-medium hover:dark:bg-white/20 hover:bg-black/20 transition-all"
              >
                Cancel
              </button>
              <button
                onClick={handleSavePaper}
                className="flex-1 px-4 py-3 bg-gradient-to-r from-purple-500 to-pink-600 text-white rounded-xl font-medium hover:from-purple-600 hover:to-pink-700 transition-all duration-300"
              >
                Add Paper
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
