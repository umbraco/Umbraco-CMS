import {defineConfig} from '@hey-api/openapi-ts';

export default defineConfig({
  input: 'https://localhost:44339/umbraco/openapi/delivery.json',
  output: './api',
  plugins: ['@hey-api/client-fetch'],
});
