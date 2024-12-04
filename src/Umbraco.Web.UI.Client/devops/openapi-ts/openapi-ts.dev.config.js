import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	client: 'fetch',
	debug: true,
	input: 'http://localhost:11000/umbraco/swagger/management/swagger.json',
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
