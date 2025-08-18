"use client";

import { AnchorDto } from "@/types/dto";
import { fmtPages } from "@/lib/utils";
import { useState } from "react";

// Mock detailed descriptions - in production, these would come from the API
const generateDetailedDescription = (contribution: string, index: number): string => {
  // This is a mock function that generates detailed descriptions
  // In a real implementation, you'd get these from your NLP service or database
  const details = [
    "This contribution introduces a novel architecture that combines attention mechanisms with hierarchical feature extraction. The approach significantly improves the model's ability to capture both local and global dependencies in the input data, leading to better performance across various benchmarks.",
    "The proposed method demonstrates substantial improvements over existing state-of-the-art approaches. Through extensive experiments on multiple datasets, we show consistent gains in accuracy, efficiency, and robustness. The method particularly excels in scenarios with limited training data.",
    "Our comprehensive evaluation framework includes both quantitative metrics and qualitative analysis. We test on standard benchmarks as well as challenging real-world scenarios, providing insights into the method's practical applicability and limitations.",
    "The implementation is designed for scalability and ease of integration. We provide detailed documentation, code examples, and pre-trained models to facilitate adoption by the research community. The modular design allows for easy customization and extension.",
    "Our validation process includes rigorous statistical testing, ablation studies, and comparison with baseline methods. We ensure reproducibility by providing detailed experimental protocols and making our code and data publicly available.",
    "This work establishes new performance benchmarks in the field, setting a high bar for future research. The results demonstrate the potential for practical applications and open up new research directions.",
    "The efficient implementation reduces computational requirements while maintaining high accuracy. Our optimizations make the approach suitable for deployment in resource-constrained environments.",
    "Through detailed analysis of various scenarios and edge cases, we provide insights into when and how the method performs best. This analysis helps practitioners make informed decisions about adoption.",
    "Our comparative study with existing methods reveals unique advantages and trade-offs. We provide clear guidelines for when our approach is most beneficial compared to alternatives.",
    "The extensive validation across multiple domains and datasets demonstrates the generalizability of our approach. Results show consistent performance improvements across diverse application areas."
  ];
  
  return details[index % details.length];
};

export default function ContributionsList({ bullets, anchors }: { bullets?: string[]; anchors?: AnchorDto[] }) {
  const [expandedItems, setExpandedItems] = useState<Set<number>>(new Set());

  const toggleExpand = (index: number) => {
    const newExpanded = new Set(expandedItems);
    if (newExpanded.has(index)) {
      newExpanded.delete(index);
    } else {
      newExpanded.add(index);
    }
    setExpandedItems(newExpanded);
  };

  if (!bullets || bullets.length === 0) return null;

  return (
    <div className="glass rounded-2xl p-6 shadow-2xl">
      <div className="mb-4 flex items-center space-x-3">
        <div className="w-8 h-8 bg-gradient-to-br from-blue-400 to-indigo-500 rounded-lg flex items-center justify-center">
          <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h2 className="text-lg font-semibold text-gray-100">Key Contributions</h2>
      </div>
      <ul className="space-y-3">
        {bullets.map((b, i) => {
          const a = anchors?.find(x => x.bulletIndex === i);
          const isExpanded = expandedItems.has(i);
          
          return (
            <li key={i} className="group relative rounded-lg border border-white/10 bg-white/5 overflow-hidden transition-all duration-300">
              {/* Main contribution card */}
              <div 
                className="p-4 hover:bg-white/10 transition-all duration-300 cursor-pointer"
                onClick={() => toggleExpand(i)}
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1 leading-7 text-gray-200 pr-3">{b}</div>
                  {/* Dropdown arrow */}
                  <div className="flex-shrink-0 w-6 h-6 flex items-center justify-center">
                    <svg 
                      className={`w-4 h-4 text-gray-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`}
                      fill="none" 
                      stroke="currentColor" 
                      viewBox="0 0 24 24"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </div>
                </div>
                
                {/* Anchor information */}
                {a && (
                  <div className="mt-2 text-xs text-gray-400">
                    <span className="rounded bg-white/10 border border-white/20 px-2 py-1 text-gray-300">{a.sectionName}</span>
                    <span className="ml-2 text-gray-400">{fmtPages(a.pageStart, a.pageEnd)}</span>
                  </div>
                )}
              </div>

              {/* Expandable detailed description */}
              <div className={`transition-all duration-300 ease-in-out ${isExpanded ? 'max-h-96 opacity-100' : 'max-h-0 opacity-0'} overflow-hidden`}>
                <div className="px-4 pb-4 border-t border-white/10">
                  <div className="pt-3 text-sm text-gray-300 leading-relaxed">
                    <div className="mb-2 text-xs font-medium text-blue-300 uppercase tracking-wide">Detailed Description</div>
                    {generateDetailedDescription(b, i)}
                  </div>
                </div>
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
