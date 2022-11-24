import { defineConfig } from 'vite';

import config from './vite.config';

export default defineConfig({
	...config,
	build: {
		lib: {
			entry: 'src/app.ts',
			formats: ['es'],
			fileName: 'main',
		},
		sourcemap: true,
	},
});
