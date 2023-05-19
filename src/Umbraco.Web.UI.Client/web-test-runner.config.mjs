import { esbuildPlugin } from '@web/dev-server-esbuild';
import { playwrightLauncher } from '@web/test-runner-playwright';
import { importMapsPlugin } from '@web/dev-server-import-maps';
import rollupUrl from 'rollup-plugin-url';
import { fromRollup } from '@web/dev-server-rollup';

const url = fromRollup(rollupUrl);

/** @type {import('@web/dev-server').DevServerConfig} */
export default {
	nodeResolve: true,
	files: ['src/**/*.test.ts', 'libs/**/*.test.ts'],
	mimeTypes: {
		'./public/**/*': 'js',
	},
	plugins: [
		{
			name: 'resolve-umbraco-and-vite-imports',
			// Rewrite Vite's root imports to the public folder
			transformImport(args) {
				if (args.source.match(/^\/.*?\.(png|gif|jpg|jpeg|svg)$/is)) {
					return `/public${args.source}`;
				}
			},
		},
		// Serve images from the public folder as JS modules
		url({ include: ['public/**/*'] }),
		esbuildPlugin({ ts: true, target: 'auto', json: true }),
		importMapsPlugin({
			inject: {
				importMap: {
					imports: {
						'src/': './src/',

						// LIBS
						'@umbraco-cms/backoffice/backend-api': './libs/backend-api/index.ts',
						'@umbraco-cms/backoffice/context-api': './libs/context-api/index.ts',
						'@umbraco-cms/backoffice/controller-api': './libs/controller-api/index.ts',
						'@umbraco-cms/backoffice/element-api': './libs/element-api/index.ts',
						'@umbraco-cms/backoffice/extension-api': './libs/extension-api/index.ts',
						'@umbraco-cms/backoffice/observable-api': './libs/observable-api/index.ts',

						// PACKAGES
						'@umbraco-cms/backoffice/resources': './libs/resources/index.ts',
						'@umbraco-cms/backoffice/content-type': './libs/content-type/index.ts',
						'@umbraco-cms/backoffice/data-type': './libs/data-type/index.ts',
						'@umbraco-cms/backoffice/entity-action': './libs/entity-action/index.ts',
						'@umbraco-cms/backoffice/events': './libs/umb-events/index.ts',
						'@umbraco-cms/backoffice/extension-registry': './libs/extension-registry/index.ts',
						'@umbraco-cms/backoffice/macro': './libs/macro/index.ts',
						'@umbraco-cms/backoffice/modal': './libs/modal/index.ts',
						'@umbraco-cms/backoffice/models': './libs/models/index.ts',
						'@umbraco-cms/backoffice/notification': './libs/notification/index.ts',
						'@umbraco-cms/backoffice/property-editor': './libs/property-editor/index.ts',
						'@umbraco-cms/backoffice/repository': './libs/repository/index.ts',
						'@umbraco-cms/backoffice/resources': './libs/resources/index.ts',
						'@umbraco-cms/backoffice/store': './libs/store/index.ts',
						'@umbraco-cms/backoffice/utils': './libs/utils/index.ts',
						'@umbraco-cms/backoffice/workspace': './libs/workspace/index.ts',
						'@umbraco-cms/backoffice/picker-input': './libs/picker-input/index.ts',
						'@umbraco-cms/backoffice/id': './libs/id/index.ts',
						'@umbraco-cms/backoffice/collection': './libs/collection/index.ts',
						'@umbraco-cms/backoffice/tree': './libs/tree/index.ts',
						'@umbraco-cms/backoffice/section': './libs/section/index.ts',
						'@umbraco-cms/backoffice/variant': './libs/variant/index.ts',

						'@umbraco-cms/backoffice/core/components': './src/packages/core/components/index.ts',
						'@umbraco-cms/backoffice/user-group': './src/packages/users/user-groups/index.ts',

						// SHARED
						'@umbraco-cms/internal/lit-element': './src/shared/lit-element/index.ts',
						'@umbraco-cms/internal/modal': './src/shared/modal/index.ts',
						'@umbraco-cms/internal/router': './src/shared/router/index.ts',
						'@umbraco-cms/internal/sorter': './src/shared/sorter/index.ts',
						'@umbraco-cms/internal/test-utils': './utils/test-utils.ts',
					},
				},
			},
		}),
	],
	browsers: [playwrightLauncher({ product: 'firefox' }), playwrightLauncher({ product: 'webkit' })],
	coverageConfig: {
		reporters: ['lcovonly', 'text-summary'],
	},
	testRunnerHtml: (testFramework) =>
		`<html>
			<head>
				<meta charset="UTF-8" />
				<meta name="viewport" content="width=device-width, initial-scale=1.0" />
				<link rel="icon" type="image/svg+xml" href="public/favicon.svg" />
				<title>Umbraco</title>
				<base href="/" />
				<script src="/node_modules/msw/lib/iife/index.js"></script>
				<link rel="stylesheet" href="node_modules/@umbraco-ui/uui-css/dist/uui-css.css">
				<link rel="stylesheet" href="src/shared/css/custom-properties.css">
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
