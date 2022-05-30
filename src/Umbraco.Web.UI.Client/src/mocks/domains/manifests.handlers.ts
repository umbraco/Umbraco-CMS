import { rest } from 'msw';

// TODO: set up schema
export const handlers = [
  rest.get('/umbraco/backoffice/manifests', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json({
        manifests: [
          {
            type: 'section',
            alias: 'My.Section.Custom',
            name: 'Custom',
            elementName: 'umb-custom-section',
            meta: {
              weight: 30,
            },
          },
        ],
      })
    );
  }),
];
