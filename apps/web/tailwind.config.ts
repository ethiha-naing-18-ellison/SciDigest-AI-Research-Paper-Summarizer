import type { Config } from "tailwindcss";

export default {
  content: ["./src/**/*.{ts,tsx}"],
  darkMode: 'class', // Enable class-based dark mode
  theme: { 
    extend: {
      colors: {
        // Custom color scheme for both themes
        background: {
          light: '#ffffff',
          dark: '#0f0f23',
        },
        surface: {
          light: '#f8fafc',
          dark: '#1a1a2e',
        },
        primary: {
          light: '#3b82f6',
          dark: '#60a5fa',
        },
        text: {
          light: '#1f2937',
          dark: '#f3f4f6',
        },
        muted: {
          light: '#6b7280',
          dark: '#9ca3af',
        }
      },
      animation: {
        'gradient-shift': 'gradientShift 15s ease infinite',
        'float': 'float 6s ease-in-out infinite',
        'pulse-glow': 'pulseGlow 2s cubic-bezier(0.4, 0, 0.6, 1) infinite',
      },
      keyframes: {
        gradientShift: {
          '0%': { backgroundPosition: '0% 50%' },
          '50%': { backgroundPosition: '100% 50%' },
          '100%': { backgroundPosition: '0% 50%' },
        },
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        pulseGlow: {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.5' },
        }
      }
    } 
  },
  plugins: [],
} satisfies Config;
