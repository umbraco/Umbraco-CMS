import { rest } from 'msw';

export const handlers = [
  rest.get('/umbraco/backoffice/init', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json({
        version: '13.0.0',
        installed: import.meta.env.VITE_UMBRACO_INSTALL_STATUS !== 'false',
      })
    );
  }),

  rest.post('/umbraco/backoffice/login', (_req, res, ctx) => {
    // Persist user's authentication in the session
    sessionStorage.setItem('is-authenticated', 'true');
    return res(
      // Respond with a 200 status code
      ctx.status(200)
    );
  }),

  rest.post('/umbraco/backoffice/logout', (_req, res, ctx) => {
    // Persist user's authentication in the session
    sessionStorage.removeItem('is-authenticated');
    return res(
      // Respond with a 200 status code
      ctx.status(200)
    );
  }),

  rest.get('/umbraco/backoffice/user', (_req, res, ctx) => {
    // Check if the user is authenticated in this session
    const isAuthenticated = sessionStorage.getItem('is-authenticated');
    if (!isAuthenticated) {
      // If not authenticated, respond with a 403 error
      return res(
        ctx.status(403),
        ctx.json({
          errorMessage: 'Not authorized',
        })
      );
    }
    // If authenticated, return a mocked user details
    return res(
      ctx.status(200),
      ctx.json({
        username: 'admin',
      })
    );
  }),

  rest.post('/umbraco/backoffice/install', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200)
    );
  }),
];
