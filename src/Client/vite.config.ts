import { defineConfig  } from 'vite'

export default defineConfig({
    base: '/dist/',
    define: {
      global: "window",
    },
    build: {
        outDir: '../Server/WebRoot/dist',
        emptyOutDir: true,
        manifest: false,
        rollupOptions: {
          
          input: {
            main: './build/App.js',
          },

        }
      },
    server: {
        watch: {
          ignored: [ "**/*.fs"]
        },

        
        https: false,
        strictPort: true,
        hmr: {
          clientPort: 5173,
          protocol: 'ws'
        }
      }
})