const { rest } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { UpgradeSettingsResponseModelReadable } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	rest.get(umbracoPath('/upgrade/settings'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<UpgradeSettingsResponseModelReadable>({
				currentState: '2b20c6e7',
				newState: '2b20c6e8',
				oldVersion: '13.0.0',
				newVersion: '17.0.0',
				reportUrl: 'https://our.umbraco.com/download/releases/1700',
			}),
		);
	}),

	rest.post(umbracoPath('/upgrade/authorize'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		return res(
			// Respond with a 200 status code
			ctx.status(201),
		);
	}),
];
