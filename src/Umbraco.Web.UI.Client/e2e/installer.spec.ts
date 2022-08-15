import { rest } from 'msw';

import umbracoPath from '../src/core/helpers/umbraco-path';
import { ProblemDetails, StatusResponse } from '../src/core/models';
import { expect, test } from '../test';

test.describe('installer tests', () => {
	test.beforeEach(async ({ page, worker }) => {
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
	});

	test('installer is shown', async ({ page }) => {
		await expect(page).toHaveURL('/install');
	});

	test.describe('test success and failure', () => {
		test.beforeEach(async ({ page }) => {
			// User form
			await expect(page.locator('[data-test="installer-user"]')).toBeVisible();
			await page.type('input[name="name"]', 'Test');
			await page.type('input[name="email"]', 'test@umbraco');
			await page.type('input[name="password"]', 'test123456');
			await page.click('[name="subscribeToNewsletter"]');

			// Go to the next step
			await page.click('[aria-label="Next"]');

			// Set telemetry
			await expect(page.locator('[data-test="installer-telemetry"]')).toBeVisible();
			expect(page.locator('[name="telemetryLevel"]')).toHaveAttribute('value', '2');

			// Click [aria-label="Next"]
			await page.click('[aria-label="Next"]');

			// Database form
			await expect(page.locator('[data-test="installer-database"]')).toBeVisible();
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
						})
					);
				})
			);

			await page.click('[aria-label="Install"]');
			const errorTxt = page.locator('#error-message');
			await expect(errorTxt).toHaveText('Something went wrong', { useInnerText: true });
		});
	});
});
