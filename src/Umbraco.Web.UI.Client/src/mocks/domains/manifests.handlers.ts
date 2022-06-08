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
            name: 'Custom Section',
            js: '/App_Plugins/section.js',
            elementName: 'my-section-custom',
            meta: {
              pathname: 'my-custom',
              weight: 1,
            },
          },
          {
            type: 'propertyEditorUI',
            alias: 'My.PropertyEditorUI.Custom',
            name: 'My Custom Property Editor UI',
            js: '/App_Plugins/property-editor.js',
            elementName: 'my-property-editor-ui-custom',
            meta: {
              icon: 'document',
              group: 'common',
            },
          },
        ],
      })
    );
  }),
];
