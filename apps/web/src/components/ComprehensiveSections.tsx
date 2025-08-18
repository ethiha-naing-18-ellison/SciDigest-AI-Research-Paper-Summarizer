"use client";

import { useState } from "react";

interface ComprehensiveSectionsProps {
  summary: {
    executiveSummary: string;
    abstract: string;
    introduction: string;
    methodology: string;
    results: string;
    discussion: string;
    limitations: string;
    technicalDetails: string;
    impact: string;
  };
}

export default function ComprehensiveSections({ summary }: ComprehensiveSectionsProps) {
  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set());

  const toggleSection = (sectionName: string) => {
    const newExpanded = new Set(expandedSections);
    if (newExpanded.has(sectionName)) {
      newExpanded.delete(sectionName);
    } else {
      newExpanded.add(sectionName);
    }
    setExpandedSections(newExpanded);
  };

  const sections = [
    {
      key: "executiveSummary",
      title: "Executive Summary",
      icon: "📋",
      content: summary.executiveSummary,
      description: "High-level overview of the research"
    },
    {
      key: "abstract",
      title: "Abstract & Overview",
      icon: "🔍",
      content: summary.abstract,
      description: "Core findings and research objectives"
    },
    {
      key: "introduction",
      title: "Introduction & Background",
      icon: "🎯",
      content: summary.introduction,
      description: "Research context and motivation"
    },
    {
      key: "methodology",
      title: "Methodology & Approach",
      icon: "⚙️",
      content: summary.methodology,
      description: "Research methods and techniques used"
    },
    {
      key: "results",
      title: "Results & Findings",
      icon: "📊",
      content: summary.results,
      description: "Key experimental results and discoveries"
    },
    {
      key: "discussion",
      title: "Discussion & Analysis",
      icon: "💭",
      content: summary.discussion,
      description: "Interpretation and implications of findings"
    },
    {
      key: "technicalDetails",
      title: "Technical Details",
      icon: "🔧",
      content: summary.technicalDetails,
      description: "Algorithms, datasets, and implementation details"
    },
    {
      key: "limitations",
      title: "Limitations & Challenges",
      icon: "⚠️",
      content: summary.limitations,
      description: "Current constraints and areas for improvement"
    },
    {
      key: "impact",
      title: "Impact & Significance",
      icon: "🌟",
      content: summary.impact,
      description: "Research contributions and future implications"
    }
  ];

  return (
    <div className="space-y-4">
      <div className="text-center mb-6">
        <h2 className="text-2xl font-bold text-gray-100 mb-2">Comprehensive Analysis</h2>
        <p className="text-gray-400">Detailed breakdown of the research paper</p>
      </div>
      
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {sections.map((section) => (
          <div
            key={section.key}
            className="glass rounded-xl p-4 shadow-lg hover:shadow-xl transition-all duration-300 cursor-pointer"
            onClick={() => toggleSection(section.key)}
          >
            <div className="flex items-center gap-3 mb-3">
              <span className="text-2xl">{section.icon}</span>
              <div>
                <h3 className="font-semibold text-gray-100 text-sm">{section.title}</h3>
                <p className="text-xs text-gray-400">{section.description}</p>
              </div>
            </div>
            
            {expandedSections.has(section.key) ? (
              <div className="mt-3">
                <p className="text-sm text-gray-300 leading-relaxed">
                  {section.content || "No content available for this section."}
                </p>
                <button
                  className="text-xs text-blue-400 hover:text-blue-300 mt-2"
                  onClick={(e) => {
                    e.stopPropagation();
                    toggleSection(section.key);
                  }}
                >
                  Show less
                </button>
              </div>
            ) : (
              <div className="mt-2">
                <p className="text-xs text-gray-400 line-clamp-2">
                  {section.content ? 
                    section.content.substring(0, 100) + (section.content.length > 100 ? "..." : "") :
                    "Click to view content"
                  }
                </p>
                <button
                  className="text-xs text-blue-400 hover:text-blue-300 mt-2"
                  onClick={(e) => {
                    e.stopPropagation();
                    toggleSection(section.key);
                  }}
                >
                  Read more
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
