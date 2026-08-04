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
			operations: {
				strategy: 'byTags',
				container: 'class',
				containerName: '{{name}}Service',
				methods: 'static',
				nesting: 'operationId',
			},
			responseStyle: 'fields',
		}
	]
});
