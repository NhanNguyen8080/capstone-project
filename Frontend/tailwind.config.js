/** @type {import('tailwindcss').Config} */
const withMT = require('@material-tailwind/react/utils/withMT');
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
    './node_modules/@material-tailwind/react/components/**/*.{js,ts,jsx,tsx}',
    './node_modules/@material-tailwind/react/theme/components/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      gridTemplateColumns: {
        '16': 'repeat(16, minmax(0, 1fr))',
      },
      fontFamily: {
        rubikmonoone: ['"Rubik Mono One"', 'monospace'],
        poppins: ['Poppins', 'sans-serif'],
        alfa: ['Alfa Slab One', 'serif'],
      },
      backgroundImage: {
        'banner': "url('/assets/images/banner-bg-3.jpg')",
        'footer': "url('/assets/images/footer.png')"
      },
    },
  },
  plugins: [],
};
