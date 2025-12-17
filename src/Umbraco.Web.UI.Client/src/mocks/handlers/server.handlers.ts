const { rest } = window.MockServiceWorker;
import { version } from '../../../package.json';
import {
	RuntimeLevelModel,
	RuntimeModeModel,
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
				allowLocalLogin: true,
				umbracoCssPath: '/css',
			}),
		);
	}),
	rest.get(umbracoPath('/server/information'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<GetServerInformationResponse>({
				version,
				assemblyVersion: version,
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
					{ name: 'Umbraco version', data: version },
				],
			}),
		);
	}),
];
