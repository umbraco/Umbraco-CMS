import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	client: 'legacy/fetch',
	debug: true,
	input: '../Umbraco.Cms.Api.Management/OpenApi.json',
	output: {
		path: 'src/external/backend-api/src',
		format: 'prettier',
		lint: 'eslint',
	},
	plugins: [
		{
			name: '@hey-api/typescript',
			enums: 'typescript'
		},
		{
			name: '@hey-api/sdk',
			asClass: true
		}
	]
});
