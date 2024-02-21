import { esbuildPlugin } from '@web/dev-server-esbuild';
import { playwrightLauncher } from '@web/test-runner-playwright';
import { importMapsPlugin } from '@web/dev-server-import-maps';
import rollupCommonjs from '@rollup/plugin-commonjs';
import { fromRollup } from '@web/dev-server-rollup';
import { createImportMap } from './devops/importmap/index.js';

const commonjs = fromRollup(rollupCommonjs);

const mode = process.env.MODE || 'dev';
if (!['dev', 'prod'].includes(mode)) {
	throw new Error(`MODE must be "dev" or "prod", was "${mode}"`);
}

/** @type {import('@web/dev-server').DevServerConfig} */
export default {
	rootDir: '.',
	files: ['./src/**/*.test.ts'],
	nodeResolve: { exportConditions: mode === 'dev' ? ['development'] : [], preferBuiltins: false, browser: true },
	browsers: [playwrightLauncher({ product: 'chromium' }), playwrightLauncher({ product: 'webkit' })],
	coverageConfig: {
		reporters: ['lcovonly', 'text-summary'],
	},
	plugins: [
		esbuildPlugin({ ts: true, tsconfig: './tsconfig.json', target: 'auto', json: true }),
		importMapsPlugin({
			inject: {
				importMap: createImportMap({
					rootDir: './src',
					additionalImports: {
						'@umbraco-cms/internal/test-utils': './utils/test-utils.ts',
					},
					replaceModuleExtensions: true,
				}),
			},
		}),
		commonjs({
			include: ['node_modules/**', 'src/external/**'],
		}),
	],
	testRunnerHtml: (testFramework, devMode) =>
		`<html lang="en-us">
			<head>
				<meta charset="UTF-8" />
				<meta name="viewport" content="width=device-width, initial-scale=1.0" />
				<link rel="icon" type="image/svg+xml" href="src/assets/favicon.svg" />
				<title>Umbraco</title>
				<base href="/" />
				<script>
					window.__UMBRACO_TEST_RUN_A11Y_TEST = ${(!devMode).toString()};
				</script>
				<script src="/node_modules/msw/lib/iife/index.js"></script>
				<link rel="stylesheet" href="node_modules/@umbraco-ui/uui-css/dist/uui-css.css">
				<link rel="stylesheet" href="src/css/umb-css.css">
			</head>
      <body>
        <script type="module" src="${testFramework}"></script>
				<script type="module">
					/* Hack to disable Lit dev mode warnings */
					const systemWarn = window.console.warn;
					window.console.warn = (...args) => {
						if (args[0].indexOf('Lit is in dev mode.') === 0) {
							return;
						}
						if (args[0].indexOf('Multiple versions of Lit loaded.') === 0) {
							return;
						}
						systemWarn(...args);
					};
				</script>
        <script type="module">
					import 'element-internals-polyfill';
					import '@umbraco-ui/uui';
        </script>
      </body>
    </html>`,
};
