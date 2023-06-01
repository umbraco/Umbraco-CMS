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

						'@umbraco-cms/backoffice/backend-api': './src/external/backend-api/index.ts',
						'@umbraco-cms/backoffice/external/uui': './src/external/uui/index.ts',
						'@umbraco-cms/backoffice/external/lit': './src/external/lit/index.ts',
						'@umbraco-cms/backoffice/external/openid': './src/external/openid/index.ts',
						'@umbraco-cms/backoffice/external/rxjs': './src/external/rxjs/index.ts',
						'@umbraco-cms/backoffice/external/router-slot': './src/external/router-slot/index.ts',
						'@umbraco-cms/backoffice/external/uuid': './src/external/uuid/index.ts',
						'@umbraco-cms/backoffice/external/lodash': './src/external/lodash/index.ts',
						'@umbraco-cms/backoffice/external/monaco-editor': './src/external/monaco-editor/index.ts',

						'@umbraco-cms/backoffice/context-api': './src/libs/context-api/index.ts',
						'@umbraco-cms/backoffice/controller-api': './src/libs/controller-api/index.ts',
						'@umbraco-cms/backoffice/element-api': './src/libs/element-api/index.ts',
						'@umbraco-cms/backoffice/extension-api': './src/libs/extension-api/index.ts',
						'@umbraco-cms/backoffice/observable-api': './src/libs/observable-api/index.ts',

						'@umbraco-cms/backoffice/events': './src/shared/umb-events/index.ts',
						'@umbraco-cms/backoffice/models': './src/shared/models/index.ts',
						'@umbraco-cms/backoffice/repository': './src/shared/repository/index.ts',
						'@umbraco-cms/backoffice/resources': './src/shared/resources/index.ts',
						'@umbraco-cms/backoffice/router': './src/shared/router/index.ts',
						'@umbraco-cms/backoffice/utils': './src/shared/utils/index.ts',
						'@umbraco-cms/backoffice/icon': './src/shared/icon-registry/index.ts',

						'@umbraco-cms/internal/lit-element': './src/shared/lit-element/index.ts',
						'@umbraco-cms/internal/modal': './src/shared/modal/index.ts',

						'@umbraco-cms/backoffice/action': './src/packages/core/action/index.ts',
						'@umbraco-cms/backoffice/collection': './src/packages/core/collection/index.ts',
						'@umbraco-cms/backoffice/components': './src/packages/core/components/index.ts',
						'@umbraco-cms/backoffice/content-type': './src/packages/core/content-type/index.ts',
						'@umbraco-cms/backoffice/debug': './src/packages/core/debug/index.ts',
						'@umbraco-cms/backoffice/entity-action': './src/packages/core/entity-action/index.ts',
						'@umbraco-cms/backoffice/entity-bulk-action': './src/packages/core/entity-bulk-action/index.ts',
						'@umbraco-cms/backoffice/extension-registry': './src/packages/core/extension-registry/index.ts',
						'@umbraco-cms/backoffice/id': './src/packages/core/id/index.ts',
						'@umbraco-cms/backoffice/menu': './src/packages/core/menu/index.ts',
						'@umbraco-cms/backoffice/modal': './src/packages/core/modal/index.ts',
						'@umbraco-cms/backoffice/notification': './src/packages/core/notification/index.ts',
						'@umbraco-cms/backoffice/picker-input': './src/packages/core/picker-input/index.ts',
						'@umbraco-cms/backoffice/section': './src/packages/core/section/index.ts',
						'@umbraco-cms/backoffice/sorter': './src/packages/core/sorter/index.ts',
						'@umbraco-cms/backoffice/store': './src/packages/core/store/index.ts',
						'@umbraco-cms/backoffice/tree': './src/packages/core/tree/index.ts',
						'@umbraco-cms/backoffice/variant': './src/packages/core/variant/index.ts',
						'@umbraco-cms/backoffice/workspace': './src/packages/core/workspace/index.ts',
						'@umbraco-cms/backoffice/property-editor': './src/packages/core/property-editor/index.ts',

						'@umbraco-cms/backoffice/document': './src/packages/documents/documents/index.ts',

						'@umbraco-cms/backoffice/data-type': './src/packages/settings/data-types/index.ts',
						'@umbraco-cms/backoffice/themes': './src/packages/settings/themes/index.ts',

						'@umbraco-cms/backoffice/user-group': './src/packages/users/user-groups/index.ts',
						"@umbraco-cms/backoffice/current-user": "./src/packages/users/current-user/index.js",
						"@umbraco-cms/backoffice/users": "./src/packages/users/users/index.js",

						'@umbraco-cms/backoffice/code-editor': './src/packages/templating/code-editor/index.ts',

						'@umbraco-cms/internal/test-utils': './utils/test-utils.ts',
					},
				},
			},
		}),
	],
	browsers: [playwrightLauncher({ product: 'chromium' }), playwrightLauncher({ product: 'webkit' })],
	coverageConfig: {
		reporters: ['lcovonly', 'text-summary'],
	},
	testRunnerHtml: (testFramework) =>
		`<html>
			<head>
				<meta charset="UTF-8" />
				<meta name="viewport" content="width=device-width, initial-scale=1.0" />
				<link rel="icon" type="image/svg+xml" href="src/assets/favicon.svg" />
				<title>Umbraco</title>
				<base href="/" />
				<script src="/node_modules/msw/lib/iife/index.js"></script>
				<link rel="stylesheet" href="node_modules/@umbraco-ui/uui-css/dist/uui-css.css">
				<link rel="stylesheet" href="src/css/umb-css.css">
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
