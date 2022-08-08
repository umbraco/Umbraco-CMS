import { rest } from 'msw';

import umbracoPath from '../../core/helpers/umbraco-path';
import { PostInstallRequest, ProblemDetails, UmbracoInstaller } from '../../core/models';

export const handlers = [
	rest.get(umbracoPath('/install/settings'), (_req, res, ctx) => {
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
								'We will send:<ul><li>Anonymized site ID, umbraco version, and packages installed.</li><li>Number of: Root nodes, Content nodes, Macros, Media, Document Types, Templates, Languages, Domains, User Group, Users, Members, and Property Editors in use.</li><li>System information: Webserver, server OS, server framework, server OS language, and database provider.</li><li>Configuration settings: Modelsbuilder mode, if custom Umbraco path exists, ASP environment, and if you are in debug mode.</li></ul><i>We might change what we send on the Detailed level in the future. If so, it will be listed above.<br>By choosing "Detailed" you agree to current and future anonymized information being collected.</i>',
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
						isConfigured: import.meta.env.VITE_UMBRACO_INSTALL_PRECONFIGURED === 'true',
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
						isConfigured: false,
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
						isConfigured: false,
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

	rest.post<PostInstallRequest>(umbracoPath('/install/settings'), async (req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		if (req.body.database?.name === 'fail') {
			return res(
				// Respond with a 200 status code
				ctx.status(400),
				ctx.json<ProblemDetails>({
					type: 'validation',
					status: 400,
					errors: {
						name: ['Database name is invalid'],
					},
				})
			);
		}
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),
];
