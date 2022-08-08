import { rest } from 'msw';

import umbracoPath from '../../core/helpers/umbraco-path';
import { PostInstallRequest, UmbracoUpgrader } from '../../core/models';

export const handlers = [
	rest.get(umbracoPath('/upgrade/settings'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<UmbracoUpgrader>({
				currentState: '2b20c6e7',
				newState: '2b20c6e8',
				oldVersion: '13.0.0',
				newVersion: '13.1.0',
				reportUrl: 'https://our.umbraco.com/download/releases/1000',
			})
		);
	}),

	rest.post<PostInstallRequest>(umbracoPath('/upgrade/authorize'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),
];
