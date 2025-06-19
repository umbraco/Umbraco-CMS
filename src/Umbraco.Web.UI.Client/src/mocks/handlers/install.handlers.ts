const { rest } = window.MockServiceWorker;
import type {
	DatabaseInstallRequestModel,
	InstallRequestModelReadable,
	InstallSettingsResponseModel,
	ProblemDetails,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TelemetryLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/install/settings'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<InstallSettingsResponseModel>({
				user: {
					minCharLength: 2,
					minNonAlphaNumericLength: 0,
					consentLevels: [
						{
							level: TelemetryLevelModel.MINIMAL,
							description: 'We will only send an anonymized site ID to let us know that the site exists.',
						},
						{
							level: TelemetryLevelModel.BASIC,
							description: 'We will send an anonymized site ID, umbraco version, and packages installed',
						},
						{
							level: TelemetryLevelModel.DETAILED,
							description:
								'We will send:<ul><li>Anonymized site ID, umbraco version, and packages installed.</li><li>Number of: Root nodes, Content nodes, Media, Document Types, Templates, Languages, Domains, User Group, Users, Members, and Property Editors in use.</li><li>System information: Webserver, server OS, server framework, server OS language, and database provider.</li><li>Configuration settings: Modelsbuilder mode, if custom Umbraco path exists, ASP environment, and if you are in debug mode.</li></ul><i>We might change what we send on the Detailed level in the future. If so, it will be listed above.<br>By choosing "Detailed" you agree to current and future anonymized information being collected.</i>',
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
						serverPlaceholder: '',
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
						providerName: 'My.Custom.Data.Provider',
						isConfigured: false,
						requiresServer: false,
						serverPlaceholder: 'undefined',
						requiresCredentials: false,
						supportsIntegratedAuthentication: false,
						requiresConnectionTest: true,
					},
				],
			}),
		);
	}),

	rest.post(umbracoPath('/install/validate-database'), async (req, res, ctx) => {
		const body = await req.json<DatabaseInstallRequestModel>();

		if (body.name === 'validate') {
			return res(
				ctx.status(400),
				ctx.json<ProblemDetails>({
					type: 'connection',
					status: 400,
					detail: 'Database connection failed',
				}),
			);
		}

		return res(
			// Respond with a 200 status code
			ctx.status(201),
		);
	}),

	rest.post(umbracoPath('/install/setup'), async (req, res, ctx) => {
		const body = await req.json<InstallRequestModelReadable>();

		if (body.database?.name === 'fail') {
			return res(
				// Respond with a 200 status code
				ctx.status(400),
				ctx.delay(1000),
				ctx.json<ProblemDetails>({
					type: 'validation',
					status: 400,
					detail: 'Something went wrong',
					errors: {
						name: ['Database name is invalid'],
					},
				}),
			);
		}
		return res(
			// Respond with a 200 status code
			ctx.status(201),
		);
	}),
];
