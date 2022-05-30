import { rest } from 'msw';
import { VersionResponse } from '../../models';

// TODO: set up schema
export const handlers = [
  rest.get('/umbraco/backoffice/version', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json<VersionResponse>({
        version: '13.0.0',
      })
    );
  }),
];
