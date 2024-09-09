const { rest } = window.MockServiceWorker;
import type {
	ServerStatusResponseModel,
	ServerInformationResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	RuntimeLevelModel,
	RuntimeModeModel,
	type ServerTroubleshootingResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const serverRunningHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusResponseModel>({
			serverStatus: RuntimeLevelModel.RUN,
		}),
	);
});

export const serverMustInstallHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusResponseModel>({
			serverStatus: RuntimeLevelModel.INSTALL,
		}),
	);
});

export const serverMustUpgradeHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusResponseModel>({
			serverStatus: RuntimeLevelModel.UPGRADE,
		}),
	);
});

export const serverInformationHandlers = [
	rest.get(umbracoPath('/server/information'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<ServerInformationResponseModel>({
				version: '14.0.0-preview004',
				assemblyVersion: '14.0.0-preview004',
				baseUtcOffset: '01:00:00',
				runtimeMode: RuntimeModeModel.BACKOFFICE_DEVELOPMENT,
			}),
		);
	}),
	rest.get(umbracoPath('/server/troubleshooting'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<ServerTroubleshootingResponseModel>({
				items: [
					{ name: 'Umbraco base url', data: location.origin },
					{ name: 'Mocked server', data: 'true' },
					{ name: 'Umbraco version', data: '14.0.0-preview004' },
				],
			}),
		);
	}),
];
