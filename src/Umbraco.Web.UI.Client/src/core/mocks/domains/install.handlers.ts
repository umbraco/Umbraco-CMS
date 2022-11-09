import { DatabaseInstall, Install, InstallSettings, ProblemDetails, TelemetryLevel } from '@umbraco-cms/backend-api';
import { umbracoPath } from '@umbraco-cms/utils';
import { rest } from 'msw';

export const handlers = [
	rest.get(umbracoPath('/install/settings'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<InstallSettings>({
				user: {
					minCharLength: 2,
					minNonAlphaNumericLength: 0,
					consentLevels: [
						{
							level: TelemetryLevel.MINIMAL,
							description: 'We will only send an anonymized site ID to let us know that the site exists.',
						},
						{
							level: TelemetryLevel.BASIC,
							description: 'We will send an anonymized site ID, umbraco version, and packages installed',
						},
						{
							level: TelemetryLevel.DETAILED,
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
						isConfigured: false,
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

	rest.post(umbracoPath('/install/validateDatabase'), async (req, res, ctx) => {
		const body = await req.json<DatabaseInstall>();

		if (body.name === 'validate') {
			return res(
				ctx.status(400),
				ctx.json<ProblemDetails>({
					type: 'connection',
					status: 400,
					detail: 'Database connection failed',
				})
			);
		}

		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),

	rest.post(umbracoPath('/install/setup'), async (req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds
		const body = await req.json<Install>();

		if (body.database?.name === 'fail') {
			return res(
				// Respond with a 200 status code
				ctx.status(400),
				ctx.json<ProblemDetails>({
					type: 'validation',
					status: 400,
					detail: 'Something went wrong',
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
