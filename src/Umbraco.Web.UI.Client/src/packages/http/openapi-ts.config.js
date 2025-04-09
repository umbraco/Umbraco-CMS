import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	debug: true,
	input: '../../../../Umbraco.Cms.Api.Management/OpenApi.json',
	output: {
		path: 'backend-api',
	},
	plugins: [
		{
			name: '@hey-api/client-fetch',
			bundle: false,
			exportFromIndex: true,
			throwOnError: true,
		},
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
