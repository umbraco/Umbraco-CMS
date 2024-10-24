import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    client: '@hey-api/client-fetch',
    input: 'https://localhost:44339/umbraco/swagger/example/swagger.json',
    output: 'src/api',
    services: {
        asClass: true,
    }
});
