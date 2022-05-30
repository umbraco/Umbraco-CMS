import { rest } from 'msw';
import { components } from '../../schemas/generated-schema';
import { handlers as installHandlers } from './domains/install.handlers';
import { handlers as manifestsHandlers } from './domains/manifests.handlers';
import { handlers as userHandlers } from './domains/user.handlers';
import { handlers as versionHandlers } from './domains/version.handlers';

export const handlers = [
  rest.get('/umbraco/backoffice/init', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json({
        installed: import.meta.env.VITE_UMBRACO_INSTALL_STATUS !== 'false',
      } as components['schemas']['InitResponse'])
    );
  }),
  ...installHandlers,
  ...manifestsHandlers,
  ...userHandlers,
  ...versionHandlers
];
