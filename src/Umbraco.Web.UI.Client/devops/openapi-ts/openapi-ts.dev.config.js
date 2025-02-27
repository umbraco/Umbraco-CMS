import { defineConfig } from '@hey-api/openapi-ts';

import defaultConfig from './openapi-ts.config';

export default defineConfig({
	...defaultConfig,
	input: 'http://localhost:11000/umbraco/swagger/management/swagger.json',
});
