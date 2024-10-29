import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { babel } from '@rollup/plugin-babel';
import path from 'node:path';

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: [
            'effector',
            'effector-react',
            '@effector/reflect',
            '@farfetched/core',
            'atomic-router',
            'atomic-router-react',
            'history',
            'patronum',
            '@picocss/pico',
            'openapi-fetch',
          ],
          react: ['react', 'react-dom'],
        },
      },
    },
  },
  css: {
    modules: {
      localsConvention: 'camelCase',
    },
  },
  plugins: [
    react(),
    babel({
      include: ['./client/**'],
      extensions: ['.ts', '.tsx'],
      babelHelpers: 'bundled',
    }),
  ],
  clearScreen: false,
  resolve: {
    alias: [
      {
        find: '~',
        replacement: path.resolve('client'),
      },
    ],
  },
  server: {
    port: 4000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/ws': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/auth': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
});
