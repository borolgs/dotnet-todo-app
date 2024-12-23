import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { babel } from '@rollup/plugin-babel';
import path from 'node:path';

const host = process.env.TAURI_DEV_HOST;

// https://vitejs.dev/config/
export default defineConfig({
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
    port: 1420,
    strictPort: true,
    host: host || false,
    hmr: host
      ? {
          protocol: 'ws',
          host,
          port: 4000,
        }
      : undefined,
  },
});
