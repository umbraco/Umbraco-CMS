import { rest } from 'msw';
import { InitResponse } from '../core/models';
import { handlers as contentHandlers } from './domains/content.handlers';
import { handlers as installHandlers } from './domains/install.handlers';
import { handlers as manifestsHandlers } from './domains/manifests.handlers';
import { handlers as userHandlers } from './domains/user.handlers';
import { handlers as versionHandlers } from './domains/version.handlers';

export const handlers = [
  rest.get('/umbraco/backoffice/init', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json<InitResponse>({
        installed: import.meta.env.VITE_UMBRACO_INSTALL_STATUS !== 'false',
      })
    );
  }),
  ...contentHandlers,
  ...installHandlers,
  ...manifestsHandlers,
  ...userHandlers,
  ...versionHandlers,
];
