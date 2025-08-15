/**
 * Tailwind Configuration (Phase 1 Foundations)
 * - Adds dark mode support (class strategy)
 * - Extends color palette with semantic brand colors
 * - Sets up container defaults & font stack
 * - Registers forms plugin for better base form control styles
 */
import forms from '@tailwindcss/forms'

/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    './index.html',
    './src/**/*.{vue,js,ts,jsx,tsx}'
  ],
  theme: {
    container: {
      center: true,
      padding: '1rem',
      screens: {
        sm: '640px',
        md: '768px',
        lg: '1024px',
        xl: '1280px'
      }
    },
    extend: {
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'Noto Sans', 'sans-serif']
      },
      colors: {
        primary: '#3498db',
        secondary: '#9b59b6',
        success: '#27ae60',
        danger: '#e74c3c'
      },
      boxShadow: {
        card: '0 2px 4px 0 rgb(0 0 0 / 0.05), 0 1px 2px -1px rgb(0 0 0 / 0.05)'
      },
      transitionDuration: {
        DEFAULT: '200ms'
      }
    }
  },
  plugins: [forms]
}
