/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        accent: '#0A7CFF',
        accentHover: '#0568D8',
        primary: '#0F172A',
        secondary: '#64748B',
        canvas: '#F6F8FC',
        panel: '#FFFFFF',
        line: '#DCE3EF'
      }
    },
  },
  plugins: [],
}
