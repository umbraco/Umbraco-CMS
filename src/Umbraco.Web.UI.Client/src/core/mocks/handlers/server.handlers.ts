import { rest } from 'msw';
import { RuntimeLevelModel, ServerStatusResponseModel, VersionResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const serverRunningHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusResponseModel>({
			serverStatus: RuntimeLevelModel.RUN,
		})
	);
});

export const serverMustInstallHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusResponseModel>({
			serverStatus: RuntimeLevelModel.INSTALL,
		})
	);
});

export const serverMustUpgradeHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusResponseModel>({
			serverStatus: RuntimeLevelModel.UPGRADE,
		})
	);
});

export const serverVersionHandler = rest.get(umbracoPath('/server/version'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<VersionResponseModel>({
			version: '13.0.0',
		})
	);
});
