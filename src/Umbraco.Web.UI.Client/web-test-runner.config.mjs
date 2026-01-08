import { esbuildPlugin } from '@web/dev-server-esbuild';
import { playwrightLauncher } from '@web/test-runner-playwright';
import { importMapsPlugin } from '@web/dev-server-import-maps';
import { createImportMap } from './devops/importmap/index.js';

const mode = process.env.MODE || 'dev';
if (!['dev', 'prod'].includes(mode)) {
	throw new Error(`MODE must be "dev" or "prod", was "${mode}"`);
}

const silencedLogs = [
	'Lit is in dev mode.',
	'Multiple versions of Lit loaded.',
	'-- Extension of alias "',
	'Error: Failed to create extension api from alias',
	'Documentation: ',
	'Found an issue? https://github.com/mswjs/msw/issues',
	'Worker script URL:',
	'Worker scope:',
];

/** @type {import('@web/dev-server').DevServerConfig} */
export default {
	rootDir: '.',
	files: ['./src/**/*.test.ts'],
	nodeResolve: { exportConditions: mode === 'dev' ? ['development'] : [], preferBuiltins: false, browser: false },
	browsers: [playwrightLauncher({ product: 'chromium' })],
	/* TODO: fix coverage report
	coverageConfig: {
		reporters: ['lcovonly', 'text-summary'],
	},
	*/
	plugins: [
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
		esbuildPlugin({ ts: true, tsconfig: './tsconfig.json', target: 'auto', json: true }),
	],
	filterBrowserLogs(log) {
		for (const arg of log.args) {
			if (typeof arg === 'string' && silencedLogs.some((l) => arg.includes(l))) {
				return false;
			}
		}
		return true;
	},
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
				<script type="module" src="/web-test-runner.index.ts"></script>
				<link rel="stylesheet" href="node_modules/@umbraco-ui/uui-css/dist/uui-css.css">
				<link rel="stylesheet" href="src/css/umb-css.css">
				<script type="module">
					import '@umbraco-cms/backoffice/components';
				</script>
			</head>
      <body>
        <script type="module" src="${testFramework}"></script>
        <script type="module">
					import 'element-internals-polyfill';
					import '@umbraco-ui/uui';
        </script>
      </body>
    </html>`,
};
