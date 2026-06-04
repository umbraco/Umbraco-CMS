import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	input: '../../../../Umbraco.Cms.Api.Management/OpenApi.json',
	output: {
		path: './backend-api',
	},
	plugins: [
		{
			name: '@hey-api/client-fetch',
			exportFromIndex: true,
			throwOnError: true,
		},
		{
			name: '@hey-api/typescript',
			enums: 'typescript'
		},
		{
			name: '@hey-api/sdk',
			responseStyle: 'fields',
			operations: {
				strategy: 'byTags',
				container: 'class',
				containerName: { name: '{{name}}Service', casing: 'PascalCase' }
			},
		}
	]
});
