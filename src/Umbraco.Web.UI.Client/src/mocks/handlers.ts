import { rest } from 'msw';

import { components } from '../../schemas/generated-schema';
import {
  AllowedSectionsResponse,
  ErrorResponse,
  InitResponse,
  UmbracoInstaller,
  UserResponse,
  VersionResponse,
} from '../models';

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

  rest.get('/umbraco/backoffice/init', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json<InitResponse>({
        installed: import.meta.env.VITE_UMBRACO_INSTALL_STATUS !== 'false',
      })
    );
  }),

  rest.get('/umbraco/backoffice/version', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(200),
      ctx.json<VersionResponse>({
        version: '13.0.0',
      })
    );
  }),

  rest.post('/umbraco/backoffice/user/login', (_req, res, ctx) => {
    // Persist user's authentication in the session
    sessionStorage.setItem('is-authenticated', 'true');
    return res(
      // Respond with a 200 status code
      ctx.status(201)
    );
  }),

  rest.post('/umbraco/backoffice/user/logout', (_req, res, ctx) => {
    // Persist user's authentication in the session
    sessionStorage.removeItem('is-authenticated');
    return res(
      // Respond with a 200 status code
      ctx.status(201)
    );
  }),

  rest.get('/umbraco/backoffice/user', (_req, res, ctx) => {
    // Check if the user is authenticated in this session
    const isAuthenticated = sessionStorage.getItem('is-authenticated');
    if (!isAuthenticated) {
      // If not authenticated, respond with a 403 error
      return res(
        ctx.status(403),
        ctx.json<ErrorResponse>({
          errorMessage: 'Not authorized',
        })
      );
    }
    // If authenticated, return a mocked user details
    return res(
      ctx.status(200),
      ctx.json<UserResponse>({
        username: 'admin',
        role: 'administrator',
      })
    );
  }),

  rest.get('/umbraco/backoffice/user/sections', (_req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json<AllowedSectionsResponse>({
        sections: ['Umb.Section.Content', 'Umb.Section.Media', 'Umb.Section.Settings', 'My.Section.Custom'],
      })
    );
  }),

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

  rest.post('/umbraco/backoffice/install', (_req, res, ctx) => {
    return res(
      // Respond with a 200 status code
      ctx.status(201)
    );
  }),
];
