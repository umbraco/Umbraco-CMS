import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	input: {
		path: '../Umbraco.Cms.Api.Management/OpenApi.json',
	},
	parser: {
		filters: {
			operations: {
				include: [
					'POST /umbraco/management/api/v1/security/forgot-password',
					'POST /umbraco/management/api/v1/security/forgot-password/verify',
					'POST /umbraco/management/api/v1/security/forgot-password/reset',
					'POST /umbraco/management/api/v1/user/invite/verify',
					'POST /umbraco/management/api/v1/user/invite/create-password',
				],
			},
		},
	},
	output: {
		path: './src/api',
	},
	plugins: [
		{
			name: '@hey-api/client-fetch',
		},
		{
			name: '@hey-api/typescript',
			enums: 'typescript',
		},
		'@hey-api/sdk',
	],
});
