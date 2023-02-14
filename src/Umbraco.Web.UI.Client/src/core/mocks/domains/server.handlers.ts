import { rest } from 'msw';
import { RuntimeLevelModel, ServerStatusModel, VersionModel } from '@umbraco-cms/backend-api';
import { umbracoPath } from '@umbraco-cms/utils';

export const serverRunningHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusModel>({
			serverStatus: RuntimeLevelModel.RUN,
		})
	);
});

export const serverMustInstallHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusModel>({
			serverStatus: RuntimeLevelModel.INSTALL,
		})
	);
});

export const serverMustUpgradeHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatusModel>({
			serverStatus: RuntimeLevelModel.UPGRADE,
		})
	);
});

export const serverVersionHandler = rest.get(umbracoPath('/server/version'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<VersionModel>({
			version: '13.0.0',
		})
	);
});
