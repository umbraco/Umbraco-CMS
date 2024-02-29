const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import {
	type ProblemDetails,
	RuntimeLevelModel,
	type ServerStatusResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { expect, test } from './test.js';

test.describe('upgrader tests', () => {
	test.beforeEach(async ({ page, worker }) => {
		await worker.use(
			// Override the server status to be "must-install"
			rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
				return res(
					// Respond with a 200 status code
					ctx.status(200),
					ctx.json<ServerStatusResponseModel>({
						serverStatus: RuntimeLevelModel.UPGRADE,
					}),
				);
			}),
		);

		await page.goto('/upgrade');
	});

	test('upgrader is shown', async ({ page }) => {
		await page.waitForSelector('[data-test="upgrader"]');
		await expect(page).toHaveURL('/upgrade');
		await expect(page.locator('h1')).toHaveText('Upgrading Umbraco', { useInnerText: true });
	});

	test('upgrader has a "View Report" button', async ({ page }) => {
		await expect(page.locator('[data-test="view-report-button"]')).toBeVisible();
	});

	test('upgrader completes successfully', async ({ page }) => {
		await page.click('[data-test="continue-button"]');
		await page.waitForSelector('umb-backoffice', { timeout: 30000 });
	});

	test('upgrader fails and shows error', async ({ page, worker }) => {
		await worker.use(
			// Override the server status to be "must-install"
			rest.post(umbracoPath('/upgrade/authorize'), (_req, res, ctx) => {
				return res(
					// Respond with a 200 status code
					ctx.status(400),
					ctx.json<ProblemDetails>({
						status: 400,
						type: 'error',
						detail: 'Something went wrong',
					}),
				);
			}),
		);

		await page.click('[data-test="continue-button"]');

		await expect(page.locator('[data-test="error-message"]')).toHaveText('Something went wrong', {
			useInnerText: true,
		});
	});
});
