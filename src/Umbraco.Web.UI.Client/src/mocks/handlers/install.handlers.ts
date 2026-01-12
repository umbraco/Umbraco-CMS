const { http, HttpResponse } = window.MockServiceWorker;
import type {
	DatabaseInstallRequestModel,
	InstallRequestModel,
	InstallSettingsResponseModel,
	ProblemDetails,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TelemetryLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath('/install/settings'), () => {
		return HttpResponse.json<InstallSettingsResponseModel>({
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
					supportsTrustServerCertificate: false,
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
					supportsTrustServerCertificate: true,
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
					supportsTrustServerCertificate: false,
					requiresConnectionTest: true,
				},
			],
		});
	}),

	http.post<object, DatabaseInstallRequestModel>(umbracoPath('/install/validate-database'), async ({ request }) => {
		const body = await request.json();

		if (body.name === 'validate') {
			return HttpResponse.json<ProblemDetails>(
				{
					type: 'connection',
					status: 400,
					detail: 'Database connection failed',
				},
				{ status: 400 },
			);
		}

		return new HttpResponse(null, { status: 201 });
	}),

	http.post<object, InstallRequestModel>(umbracoPath('/install/setup'), async ({ request }) => {
		const body = await request.json();

		if (body.database?.name === 'fail') {
			// Note: ctx.delay() is not directly supported in v2, needs to be implemented differently if delay is needed
			await new Promise((resolve) => setTimeout(resolve, 1000));
			return HttpResponse.json<ProblemDetails>(
				{
					type: 'validation',
					status: 400,
					detail: 'Something went wrong',
					errors: {
						name: ['Database name is invalid'],
					},
				},
				{ status: 400 },
			);
		}
		return new HttpResponse(null, { status: 201 });
	}),
];
