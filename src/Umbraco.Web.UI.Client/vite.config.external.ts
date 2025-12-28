/**
 * Vite config for loading extensions from an external project.
 *
 * Usage:
 *   VITE_EXTERNAL_EXTENSION=/path/to/your/extension npm run dev:external
 *
 * Optional: Add custom MSW handlers for mocking custom API endpoints:
 *   VITE_EXTERNAL_MOCKS=/path/to/your/extension/mocks npm run dev:external
 */
import { defineConfig, mergeConfig } from 'vite';
import baseConfig, { plugins } from './vite.config';
import path from 'path';

const EXTERNAL_EXTENSION_PATH = process.env.VITE_EXTERNAL_EXTENSION;
const EXTERNAL_MOCKS_PATH = process.env.VITE_EXTERNAL_MOCKS;

if (!EXTERNAL_EXTENSION_PATH) {
	console.error('\n❌ VITE_EXTERNAL_EXTENSION environment variable is required');
	console.error('   Example: VITE_EXTERNAL_EXTENSION=/path/to/my-extension npm run dev:external\n');
	process.exit(1);
}

const externalPath = path.resolve(EXTERNAL_EXTENSION_PATH);
const externalMocksPath = EXTERNAL_MOCKS_PATH ? path.resolve(EXTERNAL_MOCKS_PATH) : null;
// Fallback to empty handlers when no mocks path is provided
const mocksAliasPath = externalMocksPath || path.resolve('./src/mocks');

console.log(`\n📦 Loading external extension from: ${externalPath}`);
if (externalMocksPath) {
	console.log(`🎭 Loading external MSW handlers from: ${externalMocksPath}`);
}
console.log('');

export default mergeConfig(
	baseConfig,
	defineConfig({
		plugins: [
			...plugins,
			// Custom plugin to rewrite imports from external extension to use main project's node_modules
			{
				name: 'external-extension-resolver',
				enforce: 'pre',
				resolveId(source, importer) {
					// Only apply to files from the external extension or external mocks
					const isExternalFile = importer && (
						importer.startsWith(externalPath) ||
						(externalMocksPath && importer.startsWith(externalMocksPath))
					);
					if (isExternalFile) {
						if (source.startsWith('@umbraco-cms/backoffice') ||
							source.startsWith('lit') ||
							source.startsWith('@umbraco-ui/uui')) {
							// Resolve from main project's node_modules
							return this.resolve(source, path.resolve('./index.ts'), { skipSelf: true });
						}
					}
					return null;
				},
			},
		],
		resolve: {
			alias: {
				'@external-extension': externalPath,
				// Always define the alias - fallback to empty handlers when no mocks provided
				'@external-mocks': mocksAliasPath,
			},
		},
		server: {
			fs: {
				allow: ['.', externalPath, ...(externalMocksPath ? [externalMocksPath] : [])],
			},
		},
		define: {
			'import.meta.env.VITE_EXTERNAL_EXTENSION': JSON.stringify(externalPath),
			'import.meta.env.VITE_EXTERNAL_MOCKS': JSON.stringify(externalMocksPath || ''),
		},
		optimizeDeps: {
			// Exclude external extension's node_modules from dependency scanning
			exclude: ['puppeteer-core', '@web/dev-server-core', '@web/dev-server-esbuild', '@web/dev-server-rollup'],
		},
	}),
);
