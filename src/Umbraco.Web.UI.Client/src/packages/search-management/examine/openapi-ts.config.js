import { defineConfig } from '@hey-api/openapi-ts';

// The Examine provider exposes its own Management API document on its own route, so it has its own
// committed schema and generated client rather than riding on the core one. Regenerate with
// `npm run generate:server-api -w @umbraco-backoffice/search-management` after the schema changes.
export default defineConfig({
	input: '../../../../Umbraco.Cms.Search.Provider.Examine/OpenApi.json',
	output: {
		path: './examine/api',
		// The generated client imports its own `client/` directory, which only resolves without an
		// extension. Matches the core client, generated before the default changed.
		importFileExtension: null,
	},
	plugins: [
		{
			name: '@hey-api/client-fetch',
			exportFromIndex: true,
			throwOnError: true,
		},
		{
			name: '@hey-api/typescript',
			enums: 'typescript',
		},
		{
			name: '@hey-api/sdk',
			operations: {
				strategy: 'byTags',
				container: 'class',
				containerName: '{{name}}Service',
				methods: 'static',
				nesting: 'operationId',
			},
			responseStyle: 'fields',
		},
	],
});
