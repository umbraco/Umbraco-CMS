import { defineConfig, PluginOption } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import viteTSConfigPaths from 'vite-tsconfig-paths';
import path from 'path';

// Support external example paths (absolute paths starting with /)
const EXAMPLE_PATH = process.env.VITE_EXAMPLE_PATH;
const isExternalExample = EXAMPLE_PATH?.startsWith('/');
const externalExamplePath = isExternalExample ? path.resolve(EXAMPLE_PATH) : null;

if (externalExamplePath) {
	console.log(`\n📦 Loading external example from: ${externalExamplePath}\n`);
}

// Packages that should resolve from main project's node_modules
const SHARED_PACKAGE_PREFIXES = ['@umbraco-cms/backoffice', 'lit', '@umbraco-ui/uui'];

function isSharedPackage(source: string): boolean {
	return SHARED_PACKAGE_PREFIXES.some((prefix) => source.startsWith(prefix));
}

export const plugins: PluginOption[] = [
	viteStaticCopy({
		targets: [
			{
				src: 'public-assets/App_Plugins/*.js',
				dest: 'App_Plugins',
			},
			{
				src: 'public-assets/App_Plugins/custom-bundle-package/*.js',
				dest: 'App_Plugins/custom-bundle-package',
			},
			{
				src: 'src/css/*.css',
				dest: 'umbraco/backoffice/css',
			},
			{
				src: 'node_modules/@umbraco-ui/uui-css/dist/uui-css.css',
				dest: 'umbraco/backoffice/css',
			},
			{
				src: 'node_modules/@umbraco-ui/uui-css/assets/fonts/*',
				dest: 'umbraco/backoffice/assets/fonts',
			},
			{
				src: 'src/assets/*',
				dest: 'umbraco/backoffice/assets',
			},
			{
				src: 'src/mocks/handlers/backoffice/assets/*',
				dest: 'umbraco/backoffice/assets',
			},
			{
				src: 'node_modules/msw/lib/iife/**/*',
				dest: 'umbraco/backoffice/msw',
			},
		],
	}),
	viteTSConfigPaths(),
];

// https://vitejs.dev/config/
export default defineConfig({
	build: {
		sourcemap: true,
		rollupOptions: {
			input: {
				main: new URL('index.html', import.meta.url).pathname, // Vite should only load the main index.html file
			},
		},
	},
	plugins: [
		...plugins,
		// Resolve shared packages from main project when loading external examples
		...(externalExamplePath
			? [
					{
						name: 'external-example-resolver',
						enforce: 'pre' as const,
						resolveId(source: string, importer: string | undefined) {
							if (!importer?.startsWith(externalExamplePath)) return null;
							if (!isSharedPackage(source)) return null;
							return this.resolve(source, path.resolve('./index.ts'), { skipSelf: true });
						},
					},
				]
			: []),
	],
	// Allow Vite to serve files from external example path
	server: externalExamplePath
		? {
				fs: {
					allow: ['.', externalExamplePath],
				},
			}
		: undefined,
	// Exclude problematic dependencies from external examples
	optimizeDeps: externalExamplePath
		? {
				exclude: ['puppeteer-core', '@web/dev-server-core', '@web/dev-server-esbuild', '@web/dev-server-rollup'],
			}
		: undefined,
	// Define env vars for external extensions to access
	define: externalExamplePath
		? {
				'import.meta.env.VITE_USE_MOCK_REPO': JSON.stringify(process.env.VITE_USE_MOCK_REPO || ''),
				'import.meta.env.VITE_USE_MSW': JSON.stringify(process.env.VITE_USE_MSW || ''),
				'import.meta.env.VITE_UMBRACO_USE_MSW': JSON.stringify(process.env.VITE_UMBRACO_USE_MSW || ''),
			}
		: undefined,
});
