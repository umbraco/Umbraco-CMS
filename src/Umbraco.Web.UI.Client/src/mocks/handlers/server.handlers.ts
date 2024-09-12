const { rest } = window.MockServiceWorker;
import {
	RuntimeLevelModel,
	RuntimeModeModel,
	type GetServerUpgradeCheckResponse,
	type GetServerTroubleshootingResponse,
	type GetServerInformationResponse,
	type GetServerConfigurationResponse,
	type GetServerStatusResponse,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const serverRunningHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<GetServerStatusResponse>({
			serverStatus: RuntimeLevelModel.RUN,
		}),
	);
});

export const serverMustInstallHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<GetServerStatusResponse>({
			serverStatus: RuntimeLevelModel.INSTALL,
		}),
	);
});

export const serverMustUpgradeHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<GetServerStatusResponse>({
			serverStatus: RuntimeLevelModel.UPGRADE,
		}),
	);
});

export const serverInformationHandlers = [
	rest.get(umbracoPath('/server/configuration'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<GetServerConfigurationResponse>({
				allowPasswordReset: true,
				versionCheckPeriod: 7, // days
			}),
		);
	}),
	rest.get(umbracoPath('/server/upgrade-check'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<GetServerUpgradeCheckResponse>({
				type: 'Minor',
				comment: "14.2.0.0 is released. Upgrade today - it's free!",
				url: 'http://our.umbraco.org/contribute/releases/1420',
			}),
		);
	}),
	rest.get(umbracoPath('/server/information'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<GetServerInformationResponse>({
				version: '14.0.0',
				assemblyVersion: '14.0.0',
				baseUtcOffset: '01:00:00',
				runtimeMode: RuntimeModeModel.BACKOFFICE_DEVELOPMENT,
			}),
		);
	}),
	rest.get(umbracoPath('/server/troubleshooting'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<GetServerTroubleshootingResponse>({
				items: [
					{ name: 'Umbraco base url', data: location.origin },
					{ name: 'Mocked server', data: 'true' },
					{ name: 'Umbraco version', data: '14.0.0-preview004' },
				],
			}),
		);
	}),
];
