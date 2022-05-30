import { rest } from 'msw';
import { components } from '../../../schemas/generated-schema';

// TODO: set up schema
export const handlers = [
  rest.get('/umbraco/backoffice/version', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json({
        version: '13.0.0',
      } as components['schemas']['VersionResponse'])
    );
  }),
];