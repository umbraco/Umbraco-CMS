import { rest } from 'msw';

import umbracoPath from '../src/core/helpers/umbraco-path';
import { StatusResponse } from '../src/core/models';
import { expect, test } from '../test';

test('installer is shown', async ({ page, worker }) => {
	await worker.use(
		// Override the server status to be "must-install"
		rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
			return res(
				// Respond with a 200 status code
				ctx.status(200),
				ctx.json<StatusResponse>({
					serverStatus: 'must-install',
				})
			);
		})
	);

	await page.goto('/install');

	await page.waitForSelector('[data-test="installer"]');

	await expect(page).toHaveURL('/install');
});
