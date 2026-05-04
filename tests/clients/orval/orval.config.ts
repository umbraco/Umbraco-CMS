import {defineConfig} from 'orval';

export default defineConfig({
  'umbraco-delivery': {
    input: {
      target: 'https://localhost:44339/umbraco/openapi/delivery.json',
      validation: false,
    },
    output: {
      target: 'api/umbraco-delivery.ts',
      baseUrl: 'https://localhost:44339',
    },
  },
});
