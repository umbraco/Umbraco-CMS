import { rest } from 'msw';
import { RuntimeLevel, ServerStatus, Version } from '@umbraco-cms/backend-api';
import { umbracoPath } from '@umbraco-cms/utils';

export const serverRunningHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatus>({
			serverStatus: RuntimeLevel.RUN,
		})
	);
});

export const serverMustInstallHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatus>({
			serverStatus: RuntimeLevel.INSTALL,
		})
	);
});

export const serverMustUpgradeHandler = rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ServerStatus>({
			serverStatus: RuntimeLevel.UPGRADE,
		})
	);
});

export const serverVersionHandler = rest.get(umbracoPath('/server/version'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<Version>({
			version: '13.0.0',
		})
	);
});
