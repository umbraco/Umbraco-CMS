const { rest } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import {
	type ProblemDetails,
	RuntimeLevelModel,
	type ServerStatusResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { expect, test } from './test.js';

test.describe('installer tests', () => {
	test.beforeEach(async ({ page, worker }) => {
		await worker.use(
			// Override the server status to be "must-install"
			rest.get(umbracoPath('/server/status'), (_req, res, ctx) => {
				return res(
					// Respond with a 200 status code
					ctx.status(200),
					ctx.json<ServerStatusResponseModel>({
						serverStatus: RuntimeLevelModel.INSTALL,
					}),
				);
			}),
		);

		await page.goto('/install');

		await page.waitForSelector('[data-test="installer"]');
	});

	test('installer is shown', async ({ page }) => {
		await expect(page).toHaveURL('/install');
	});

	test.describe('test success and failure', () => {
		test.beforeEach(async ({ page }) => {
			await page.waitForSelector('[data-test="installer-user"]');
			await page.fill('[aria-label="name"]', 'Test');
			await page.fill('[aria-label="email"]', 'test@umbraco');
			await page.fill('[aria-label="password"]', 'test123456');
			await page.click('[name="subscribeToNewsletter"]');

			// Go to the next step
			await page.click('[aria-label="Next"]');

			// Set telemetry
			await page.waitForSelector('[data-test="installer-telemetry"]');
			await page.waitForSelector('uui-slider[name="telemetryLevel"]');

			// Click [aria-label="Next"]
			await page.click('[aria-label="Next"]');

			// Database form
			await page.waitForSelector('[data-test="installer-database"]');
		});

		test('installer completes successfully', async ({ page }) => {
			await page.click('[aria-label="Install"]');
			await page.waitForSelector('umb-backoffice', { timeout: 30000 });
		});

		test('installer fails', async ({ page, worker }) => {
			await worker.use(
				// Override the server status to be "must-install"
				rest.post(umbracoPath('/install/setup'), (_req, res, ctx) => {
					return res(
						// Respond with a 200 status code
						ctx.status(400),
						ctx.json<ProblemDetails>({
							status: 400,
							type: 'validation',
							detail: 'Something went wrong',
							errors: {
								databaseName: ['The database name is required'],
							},
						}),
					);
				}),
			);

			await page.click('[aria-label="Install"]');

			await page.waitForSelector('[data-test="installer-error"]');

			await expect(page.locator('[data-test="error-message"]')).toHaveText('Something went wrong', {
				useInnerText: true,
			});

			// Click reset button
			await page.click('#button-reset');

			await page.waitForSelector('[data-test="installer-user"]');
		});
	});
});
