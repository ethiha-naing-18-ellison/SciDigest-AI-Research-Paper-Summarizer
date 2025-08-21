"use client";

import { useState, useEffect } from 'react';

export default function ThemeToggle() {
  const [mounted, setMounted] = useState(false);
  const [theme, setTheme] = useState<'light' | 'dark'>('dark');

  useEffect(() => {
    setMounted(true);
    
    // Load theme from localStorage or default to dark
    const savedTheme = localStorage.getItem('scidigest-theme') as 'light' | 'dark';
    if (savedTheme) {
      setTheme(savedTheme);
    }
  }, []);

  const toggleTheme = () => {
    const newTheme = theme === 'light' ? 'dark' : 'light';
    setTheme(newTheme);
    localStorage.setItem('scidigest-theme', newTheme);
    
    // Apply theme to document
    document.documentElement.classList.remove('light', 'dark');
    document.documentElement.classList.add(newTheme);
    
    // Update CSS custom properties
    const root = document.documentElement;
    if (newTheme === 'light') {
      root.style.setProperty('--color-scheme', 'light');
    } else {
      root.style.setProperty('--color-scheme', 'dark');
    }
  };

  if (!mounted) {
    // Return a placeholder with the same dimensions to prevent layout shift
    return (
      <div className="w-16 h-8 rounded-full bg-white/10 border border-white/20"></div>
    );
  }

  return (
    <button
      onClick={toggleTheme}
      className={`
        relative w-16 h-8 rounded-full p-1 transition-all duration-300 ease-in-out
        ${theme === 'dark' 
          ? 'bg-gradient-to-r from-slate-700 to-slate-800 border border-slate-600' 
          : 'bg-gradient-to-r from-yellow-400 to-orange-500 border border-yellow-300'
        }
        hover:scale-105 focus:outline-none focus:ring-2 focus:ring-offset-2 
        ${theme === 'dark' ? 'focus:ring-slate-500' : 'focus:ring-yellow-400'}
        shadow-lg hover:shadow-xl
      `}
      aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
    >
      {/* Toggle circle */}
      <div
        className={`
          absolute top-1 w-6 h-6 rounded-full transition-all duration-300 ease-in-out
          flex items-center justify-center text-xs font-bold
          ${theme === 'dark'
            ? 'left-1 bg-slate-200 text-slate-800 shadow-md'
            : 'left-9 bg-white text-orange-600 shadow-md'
          }
        `}
      >
        {theme === 'dark' ? (
          // Moon icon
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" clipRule="evenodd" />
          </svg>
        ) : (
          // Sun icon
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l.707.707a1 1 0 001.414-1.414l-.707-.707a1 1 0 00-1.414 1.414zm2.12-10.607a1 1 0 010 1.414l-.706.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.465 5.05l-.708-.707a1 1 0 00-1.414 1.414l.707.707zm1.414 8.486l-.707.707a1 1 0 01-1.414-1.414l.707-.707a1 1 0 011.414 1.414zM4 11a1 1 0 100-2H3a1 1 0 000 2h1z" clipRule="evenodd" />
          </svg>
        )}
      </div>

      {/* Background decorative elements */}
      <div className={`
        absolute inset-0 rounded-full opacity-20 transition-opacity duration-300
        ${theme === 'dark' 
          ? 'bg-gradient-to-r from-blue-600 to-purple-600' 
          : 'bg-gradient-to-r from-yellow-300 to-orange-400'
        }
      `}></div>
    </button>
  );
}
