import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	input: {
		include:
			'(ProblemDetails|ReferenceByIdModel|ResetPassword|PasswordConfiguration|SecurityConfiguration|InviteUser|CreateInitialPasswordUser|/security/|/user/invite/)',
		path: '../Umbraco.Cms.Api.Management/OpenApi.json',
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
