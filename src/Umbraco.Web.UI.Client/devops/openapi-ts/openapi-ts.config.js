import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	client: 'fetch',
	input: 'https://raw.githubusercontent.com/umbraco/Umbraco-CMS/v14/dev/src/Umbraco.Cms.Api.Management/OpenApi.json',
	output: {
		path: 'src/external/backend-api/src',
		format: 'prettier',
		lint: 'eslint',
	},
	schemas: false,
	services: {
		asClass: true,
	},
	types: {
		enums: 'typescript',
	},
});
