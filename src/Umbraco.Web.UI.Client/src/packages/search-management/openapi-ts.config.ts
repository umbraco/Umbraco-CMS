import { defineConfig } from '@hey-api/openapi-ts';

// The Examine provider exposes its own Management API document on its own route, so it has its own
// committed schema. Only the models are generated: requests go through `umbHttpClient`, which is
// already configured for back-office auth, so generating an SDK would vendor a second copy of the
// hey-api fetch runtime for a single endpoint. Paths are relative to this package's directory.
export default defineConfig({
	input: '../../../../Umbraco.Cms.Search.Provider.Examine/OpenApi.json',
	output: {
		path: './examine/api',
	},
	plugins: [
		{
			name: '@hey-api/typescript',
			enums: 'typescript',
		},
	],
});
