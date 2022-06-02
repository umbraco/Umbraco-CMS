import { rest } from 'msw';
import { ErrorResponse, UmbracoInstaller, UmbracoPerformInstallRequest } from '../../core/models';

export const handlers = [
  rest.get('/umbraco/backoffice/install', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json<UmbracoInstaller>({
        user: {
          minCharLength: 2,
          minNonAlphaNumericLength: 0,
          consentLevels: [
            {
              level: 'Minimal',
              description: 'We will only send an anonymized site ID to let us know that the site exists.',
            },
            {
              level: 'Basic',
              description: 'We will send an anonymized site ID, umbraco version, and packages installed',
            },
            {
              level: 'Detailed',
              description:
                'We will send:\n          <br>- Anonymized site ID, umbraco version, and packages installed.\n          <br>- Number of: Root nodes, Content nodes, Macros, Media, Document Types, Templates, Languages, Domains, User Group, Users, Members, and Property Editors in use.\n          <br>- System information: Webserver, server OS, server framework, server OS language, and database provider.\n          <br>- Configuration settings: Modelsbuilder mode, if custom Umbraco path exists, ASP environment, and if you are in debug mode.\n          <br>\n          <br><i>We might change what we send on the Detailed level in the future. If so, it will be listed above.\n          <br>By choosing "Detailed" you agree to current and future anonymized information being collected.</i>',
            },
          ],
        },
        databases: [
          {
            id: '1',
            sortOrder: -1,
            displayName: 'SQLite',
            defaultDatabaseName: 'Umbraco',
            providerName: 'Microsoft.Data.SQLite',
            isAvailable: true,
            requiresServer: false,
            serverPlaceholder: null,
            requiresCredentials: false,
            supportsIntegratedAuthentication: false,
            requiresConnectionTest: false,
          },
          {
            id: '2',
            sortOrder: 2,
            displayName: 'SQL Server',
            defaultDatabaseName: '',
            providerName: 'Microsoft.Data.SqlClient',
            isAvailable: true,
            requiresServer: true,
            serverPlaceholder: '(local)\\SQLEXPRESS',
            requiresCredentials: true,
            supportsIntegratedAuthentication: true,
            requiresConnectionTest: true,
          },
          {
            id: '42c0eafd-1650-4bdb-8cf6-d226e8941698',
            sortOrder: 2147483647,
            displayName: 'Custom',
            defaultDatabaseName: '',
            providerName: null,
            isAvailable: true,
            requiresServer: false,
            serverPlaceholder: null,
            requiresCredentials: false,
            supportsIntegratedAuthentication: false,
            requiresConnectionTest: true,
          },
        ],
      })
    );
  }),

  rest.post<UmbracoPerformInstallRequest>('/umbraco/backoffice/install', async (req, res, ctx) => {
    await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

    if (req.body.database.databaseName === 'fail') {
      return res(
        // Respond with a 200 status code
        ctx.status(400),
        ctx.json<ErrorResponse>({
          errorMessage: 'Database name is invalid',
        })
      );
    }
    return res(
      // Respond with a 200 status code
      ctx.status(201)
    );
  }),
];
