import { esbuildPlugin } from '@web/dev-server-esbuild';
import { playwrightLauncher } from '@web/test-runner-playwright';
import { importMapsPlugin } from '@web/dev-server-import-maps';

export default {
	nodeResolve: true,
	files: 'src/**/*.test.ts',
	plugins: [
		esbuildPlugin({ ts: true, target: 'auto' }),
		importMapsPlugin({
			inject: {
				importMap: {
					imports: {
						'@umbraco-cms/models': './src/core/models/index.ts',
						'@umbraco-cms/backend-api': './src/core/backend-api/index.ts',
						'@umbraco-cms/context-api': './src/core/context-api/index.ts',
						'@umbraco-cms/extensions-api': './src/core/extensions-api/index.ts',
						'@umbraco-cms/observable-api': './src/core/observable-api/index.ts',
						'@umbraco-cms/utils': './src/core/utils/index.ts',
						'@umbraco-cms/test-utils': './src/core/test-utils/index.ts',
						'@umbraco-cms/extensions-registry': './src/core/extensions-registry/index.ts',
					},
				},
			},
		}),
	],
	browsers: [
		playwrightLauncher({ product: 'chromium' }),
		playwrightLauncher({ product: 'firefox' }),
		playwrightLauncher({ product: 'webkit' }),
	],
	coverageConfig: {
		reporters: ['lcovonly', 'text-summary'],
	},
	testRunnerHtml: (testFramework) =>
		`<html>
			<head>
				<link rel="stylesheet" href="/src/core/css/custom-properties.css">
			</head>
      <body>
        <script type="module" src="${testFramework}"></script>
        <script type="module">
					import 'element-internals-polyfill';
					import '@umbraco-ui/uui';
					import 'router-slot';
        </script>
      </body>
    </html>`,
};
