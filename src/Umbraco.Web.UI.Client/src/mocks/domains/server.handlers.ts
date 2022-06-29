import { rest } from 'msw';

import { StatusResponse, VersionResponse } from '../../core/models';

export const handlers = [
  rest.get('/umbraco/backoffice/server/status', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json<StatusResponse>({
        serverStatus: import.meta.env.VITE_UMBRACO_INSTALL_STATUS !== 'false' ? 'running' : 'must-install',
      })
    );
  }),
  rest.get('/umbraco/backoffice/server/version', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json<VersionResponse>({
        version: '13.0.0',
      })
    );
  }),
];
