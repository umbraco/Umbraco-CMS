import { defineConfig } from 'vite';
import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));

// In dev (`vite serve`), Vite needs to resolve `@umbraco-cms/backoffice/*` to
// real files so the browser can fetch them — there's no host-page importmap
// during standalone dev like there is in production. Mirror tsc's resolution
// by aliasing each path to the same target tsconfig.json's `paths` declares.
// In build mode (lib build), `rollupOptions.external` below leaves the imports
// untouched so the host page's importmap can satisfy them at runtime.
function readTsconfigAliases(): Record<string, string> {
	const raw = readFileSync(resolve(here, 'tsconfig.json'), 'utf8');
	// tsconfig.json has a JSDoc-style header before the JSON object — strip it.
	const json = JSON.parse(raw.replace(/^[\s\S]*?(?=\{)/, ''));
	const paths: Record<string, string[]> = json.compilerOptions?.paths ?? {};
	return Object.fromEntries(
		Object.entries(paths).map(([key, [target]]) => [key, resolve(here, target)]),
	);
}

// https://vitejs.dev/config/
export default defineConfig(({ command }) => ({
	resolve: command === 'serve' ? { alias: readTsconfigAliases() } : undefined,
	build: {
		lib: {
			entry: 'src/index.ts',
			formats: ['es'],
			fileName: 'login',
		},
		rollupOptions: {
			external: [/^@umbraco-cms/],
		},
		target: 'esnext',
		sourcemap: true,
		outDir: '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/login',
		emptyOutDir: true,
	},
}));
